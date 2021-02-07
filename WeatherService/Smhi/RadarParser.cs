using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Common;
using Common.Config;
using Common.File;
using Common.Redis;
using Common.Response;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using NodaTime;
using NodaTime.Serialization.JsonNet;
using NodaTime.Text;

using WeatherService.Data;
using WeatherService.Json;

namespace WeatherService.Smhi
{
    public class RadarParser : IResponseParser
    {
        private readonly IRedisCacheService redis;
        private readonly FileDownloadParser fileDownloadParser;
        private readonly ILogger logger;
        private readonly InstantPattern timeStampPattern = InstantPattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm");

        public RadarParser(MinioConfiguration _minioConfiguration, IRedisCacheService _redis, ILoggerFactory _loggerFactory)
        {
            fileDownloadParser = new FileDownloadParser(_minioConfiguration, _loggerFactory);
            redis = _redis;
            logger = _loggerFactory.CreateLogger<RadarParser>();
        }

        public async Task<IResponse> ParseAsync(HttpContent _responseContent)
        {
            async Task<IResponse> ParseRadar()
            {
                try
                {
                    var response = await _responseContent.ReadAsStringAsync();

                    var serializer = new JsonSerializer();
                    serializer.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

                    using var jsonReader = new JsonTextReader(new StringReader(response));

                    var jRadar = serializer.Deserialize<JRadar>(jsonReader);

                    if (jRadar == null)
                    {
                        logger.LogError("No radar received");
                        return FailureResponse.Instance;
                    }

                    var result = new List<RadarImageResponse>();

                    foreach (var file in jRadar.Files)
                    {
                        var imageUrl = file.Formats.First().Link;
                        var key = file.Key;
                        var timeStamp = file.Valid;

                        var redisResult = await redis.GetValue<RadarImageResponse>($"weather_radar_image:{key}");

                        if (redisResult.Success)
                        {
                            var redisRadarImage = (RedisResponse<RadarImageResponse>) redisResult;
                            result.Add(redisRadarImage.Value);
                            logger.LogDebug("Radar image with key {Key} already downloaded, skipping", key);
                            continue;
                        }

                        var imageResult = await RestRequestHandler.SendGetRequestAsync(imageUrl, fileDownloadParser, logger);

                        if (!imageResult.Success)
                            continue;

                        var parseResult = timeStampPattern.Parse(timeStamp);

                        result.Add(new RadarImageResponse
                        {
                            TimeStamp = parseResult.Success ? parseResult.Value : default,
                            OriginUrl = imageUrl,
                            Key = key,
                            ImageUrl = ((FileDownloadResponse) imageResult).FileUri
                        });
                    }

                    return new RadarResponse
                    {
                        RadarImages = result
                    };
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to download any radar images");
                }

                return FailureResponse.Instance;
            }

            return await Task.Run(ParseRadar);
        }
    }
}