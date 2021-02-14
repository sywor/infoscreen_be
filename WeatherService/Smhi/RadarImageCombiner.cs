using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Common.Config;
using Common.Minio;
using Common.Redis;

using ImageMagick;

using Microsoft.Extensions.Logging;

using NodaTime;
using NodaTime.Text;

using WeatherService.Data;

namespace WeatherService.Smhi
{
    public class RadarImageCombiner
    {
        private readonly IRedisCacheService redis;
        private readonly ILogger logger;
        private readonly MinioService minioService;
        private readonly InstantPattern instantPattern = InstantPattern.CreateWithInvariantCulture("yyMMddHHmm");
        private const string Bucket = "ephemeral";
        private const string Directory = "radar";

        public RadarImageCombiner(ILoggerFactory _loggerFactory, MinioConfiguration _minioConfiguration, IRedisCacheService _redis)
        {
            redis = _redis;
            logger = _loggerFactory.CreateLogger<RadarImageCombiner>();
            minioService = new MinioService(_minioConfiguration);
        }

        public async Task<MinioFile> GenerateRadar()
        {
            var mask = await minioService.GetStaticObjectAsync<MagickImage, MagickImageReceiver>(MinioFile.Of(Bucket, Directory, "mask.png"));
            var background = await minioService.GetStaticObjectAsync<MagickImage, MagickImageReceiver>(MinioFile.Of(Bucket, Directory, "basemap.png"));
            var outline = await minioService.GetStaticObjectAsync<MagickImage, MagickImageReceiver>(MinioFile.Of(Bucket, Directory, "outlines.png"));

            var keys = await redis.GetKeys("weather_radar_image:*");

            var filteredKeys = keys.Select(_key =>
                                   {
                                       var capture = Regex.Match(_key, @"(\d{10})").Captures.First().Value;
                                       var timestamp = instantPattern.Parse(capture).Value;
                                       return (Key: _key, Timestamp: timestamp);
                                   }).OrderBy<(string, Instant), Instant>(_tuple => _tuple.Item2)
                                   .TakeLast(72)
                                   .Select(_tuple => _tuple.Item1);

            var radarImageResponses = new List<RadarImageResponse>();

            foreach (var key in filteredKeys)
            {
                var redisResponse = await redis.GetValue<RadarImageResponse>(key);

                if (!redisResponse.Success)
                    continue;

                radarImageResponses.Add(((RedisResponse<RadarImageResponse>)redisResponse).Value);
            }

            var radarImageComposites = new List<MagickImage>();

            foreach (var radarImageResponse in radarImageResponses)
            {
                var magickImage = await minioService.GetEphemeralObjectAsync<MagickImage, MagickImageReceiver>(radarImageResponse.FileLocation);
                var radarImageComposite = CreateRadarImageComposite(magickImage, mask, background, outline, radarImageResponse.TimeStamp);
                radarImageComposites.Add(radarImageComposite);
            }

            var result = await GenerateGif(radarImageComposites);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return result;
        }

        private static MagickImage CreateRadarImageComposite(MagickImage _rawRadarImage, MagickImage _mask, MagickImage _background, MagickImage _outline, Instant _createdAt)
        {
            _mask.Transparent(MagickColors.White);
            _rawRadarImage.Composite(_mask, CompositeOperator.CopyAlpha);
            _rawRadarImage.Transparent(MagickColors.Black);

            var images = new MagickImageCollection
            {
                _background,
                _rawRadarImage,
                _outline
            };

            using var result = images.Mosaic();
            result.Transparent(MagickColors.White);

            new Drawables()

                // Draw text on the image
                .FontPointSize(40)
                .Font("Arial", FontStyleType.Any, FontWeight.Normal, FontStretch.Expanded)
                .StrokeColor(MagickColors.White)
                .StrokeAntialias(true)
                .FillColor(MagickColors.White)
                .TextAntialias(true)
                .TextAlignment(TextAlignment.Left)
                .Text(0, 875, _createdAt.ToString("dddd HH", CultureInfo.InvariantCulture))
                .Draw(result);

            result.Scale(new Percentage(59.0));

            return new MagickImage(result);
        }

        private async Task<MinioFile> GenerateGif(IEnumerable<MagickImage> _images)
        {
            try
            {
                logger.LogInformation("Generating new gif");

                var stopWatch = Stopwatch.StartNew();

                using var gif = new MagickImageCollection();

                foreach (var radarImage in _images)
                {
                    gif.Add(radarImage);
                    gif.Last().AnimationDelay = 5;
                }

                gif.Last().AnimationDelay = 50;

                gif.OptimizePlus();
                gif.OptimizeTransparency();

                var fileName = $"{Guid.NewGuid()}_smhi.gif";

                logger.LogInformation("Saving");

                var saveStream = new MemoryStream();
                await gif.WriteAsync(saveStream, MagickFormat.Gif);

                var minioFile = MinioFile.Of(Bucket, Directory, fileName);
                await minioService.PutEphemeralObjectAsync(minioFile, saveStream, "image/gif");

                logger.LogInformation("Saving done");

                stopWatch.Stop();
                logger.LogInformation("Update radar gif, took: {Took}", $"{stopWatch.Elapsed:g}");

                return minioFile;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate new radar image");
            }

            return default;
        }

    }
}