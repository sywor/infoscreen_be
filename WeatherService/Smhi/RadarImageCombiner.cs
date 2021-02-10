using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Common.Config;
using Common.Minio;
using Common.Redis;

using ImageMagick;

using Microsoft.Extensions.Logging;

using NodaTime;

using WeatherService.Data;

namespace WeatherService.Smhi
{
    public class RadarImageCombiner
    {
        private readonly IRedisCacheService redis;
        private readonly ILogger logger;
        private readonly MinioService minioService;

        public RadarImageCombiner(ILoggerFactory _loggerFactory, MinioConfiguration _minioConfiguration, IRedisCacheService _redis)
        {
            redis = _redis;
            logger = _loggerFactory.CreateLogger<RadarImageCombiner>();
            minioService = new MinioService(_minioConfiguration);
        }

        public async Task<MinioFile> GenerateRadar()
        {
            var mask = await minioService.GetStaticObjectAsync<MagickImage, MagickImageReceiver>(MinioFile.Of("radar", "mask.png"));
            var background = await minioService.GetStaticObjectAsync<MagickImage, MagickImageReceiver>(MinioFile.Of("radar", "basemap.png"));
            var outline = await minioService.GetStaticObjectAsync<MagickImage, MagickImageReceiver>(MinioFile.Of("radar", "outlines.png"));

            var keys = await redis.GetKeys("weather_radar_image:*");

            var radarImageComposites = new List<MagickImage>();

            foreach (var key in keys)
            {
                var redisResponse = await redis.GetValue<RadarImageResponse>(key);

                if (!redisResponse.Success)
                    continue;

                var radarImageResponse = ((RedisResponse<RadarImageResponse>) redisResponse).Value;
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

                gif.Optimize();
                gif.OptimizeTransparency();

                var fileName = $"{Guid.NewGuid()}_smhi.gif";

                logger.LogInformation("Saving");

                await using var saveStream = new MemoryStream();
                await gif.WriteAsync(saveStream);
                var minioFile = MinioFile.Of("radar", fileName);
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