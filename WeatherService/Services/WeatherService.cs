using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Common.Minio;
using Common.Redis;
using Common.Response;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.Extensions.Logging;

using NodaTime;
using NodaTime.Serialization.Protobuf;

using WeatherService.Data;

namespace WeatherService.Services
{
    public class WeatherService : WeatherFetcher.WeatherFetcherBase
    {
        private readonly IRedisCacheService redis;
        private readonly ILogger<WeatherService> logger;

        public WeatherService(ILoggerFactory _loggerFactory, IRedisCacheService _redis)
        {
            redis = _redis;
            logger = _loggerFactory.CreateLogger<WeatherService>();
        }

        public override async Task<WeatherResponseProto> GetLatestWeatherReport(Empty _request, ServerCallContext _context)
        {
            var keys = await redis.GetKeys("weather_report:*");

            var filteredKey = keys.Select(_key =>
                                  {
                                      var capture = Regex.Match(_key, @"(\d{10})").Captures.First().Value;
                                      var timestamp = Instant.FromUnixTimeSeconds(int.Parse(capture));
                                      return (Key: _key, Timestamp: timestamp);
                                  }).OrderBy<(string, Instant), Instant>(_tuple => _tuple.Item2)
                                  .Select(_tuple => _tuple.Item1)
                                  .Last();

            var redisResponse = await redis.GetValue<WeatherReport>(filteredKey);

            return redisResponse.Success ? CreateWeatherResponseProto(redisResponse) : CreateFailureResponseProto(filteredKey);
        }

        public override async Task GetAllWeatherReports(Empty _request, IServerStreamWriter<WeatherResponseProto> _responseStream, ServerCallContext _context)
        {
            var keys = await redis.GetKeys("weather_report:*");

            var orderedKeys = keys.Select(_key =>
                                  {
                                      var capture = Regex.Match(_key, @"(\d{10})").Captures.First().Value;
                                      var timestamp = Instant.FromUnixTimeSeconds(int.Parse(capture));
                                      return (Key: _key, Timestamp: timestamp);
                                  }).OrderBy<(string, Instant), Instant>(_tuple => _tuple.Item2)
                                  .Select(_tuple => _tuple.Item1);

            foreach (var orderedKey in orderedKeys)
            {
                var redisResponse = await redis.GetValue<WeatherReport>(orderedKey);

                if (!redisResponse.Success)
                {
                    await _responseStream.WriteAsync(CreateFailureResponseProto(orderedKey));
                }

                await _responseStream.WriteAsync(CreateWeatherResponseProto(redisResponse));
            }
        }

        private static WeatherResponseProto CreateWeatherResponseProto(IResponse _redisResponse)
        {

            var weatherReport = ((RedisResponse<WeatherReport>)_redisResponse).Value;
            var currentWeather = weatherReport.CurrentWeather;
            var weatherForecast = weatherReport.WeatherForecast;

            var weatherForecastProto = CreateForecastProto(weatherForecast);

            var precipitationProto = new PrecipitationProto()
            {
                LastHour = currentWeather.Precipitation.First(),
                Last3Hours = currentWeather.Precipitation.Take(3).Sum(),
                Last12Hours = currentWeather.Precipitation.Take(12).Sum(),
                Last24Hours = currentWeather.Precipitation.Sum()
            };

            return new WeatherResponseProto
            {
                Weather = new WeatherProto
                {
                    Humidity = currentWeather.Humidity,
                    Temperature = currentWeather.Temperature,
                    Visibility = currentWeather.Visibility,
                    CloudCover = currentWeather.CloudCover,
                    PrecipitationValue = precipitationProto,
                    RadarImage = MinioFileToProto(weatherReport.RadarFileLocation),
                    WeatherIcon = MinioFileToProto(ResourceRegister.LookupWeatherSymbol(currentWeather.WeatherIcon)),
                    WindDirection = MinioFileToProto(ResourceRegister.LookupWindDirection(currentWeather.WindDirectionIcon)),
                    WindSpeed = currentWeather.WindSpeed,
                    WindGustSpeed = currentWeather.WindGustSpeed,
                    WeatherForecast = weatherForecastProto,
                    WeatherDescription = ResourceRegister.LookupWeatherDescription(currentWeather.WeatherIcon)
                }
            };
        }

        private static WeatherForecastProto CreateForecastProto(WeatherForecast _weatherForecast)
        {
            var weatherForecastProto = new WeatherForecastProto
            {
                StartTime = _weatherForecast.StartDate.ToTimestamp(),
                EndTime = _weatherForecast.EndDate.ToTimestamp(),
                MaxTemp = _weatherForecast.MaxTemp,
                MinTemp = _weatherForecast.MinTemp,
                MaxWindSpeed = _weatherForecast.MaxWindSpeed,
                MinWindSpeed = _weatherForecast.MinWindSpeed,
            };

            var bars = _weatherForecast.WeatherForecastBars.Select(_bar => new BarProto
            {
                Precipitation = _bar.PrecipitationValue,
                Temperature = _bar.Temprature,
                WindSpeed = _bar.WindSpeed,
                WeatherIcon = MinioFileToProto(ResourceRegister.LookupWeatherSymbol(_bar.WeatherSymbol)),
                WindDirectionIcon = MinioFileToProto(ResourceRegister.LookupWindDirection(_bar.WindDirection))
            }).ToList();

            weatherForecastProto.Forecast.AddRange(bars);
            return weatherForecastProto;
        }

        private static ProtoMinioFile MinioFileToProto(MinioFile _minioFile)
        {
            var radarProto = new ProtoMinioFile
            {
                Bucket = _minioFile.Bucket,
                Directory = _minioFile.Directory,
                FileName = _minioFile.FileName
            };
            return radarProto;
        }

        private static WeatherResponseProto CreateFailureResponseProto(string _key)
        {
            return new()
            {
                Status = new StatusProto
                {
                    Code = StatusCode.Failure,
                    Message = $"Could not find any weather report with key: {_key}"
                }
            };
        }
    }
}