using System.Threading.Tasks;

using Common;
using Common.Config;
using Common.Recurrence;
using Common.Redis;

using Microsoft.Extensions.Logging;

using NodaTime;

using WeatherService.Data;

namespace WeatherService.Smhi
{
    public class SmhiFetcher : IRunnable
    {
        private readonly IRedisCacheService redis;
        private readonly ILogger<SmhiFetcher> logger;
        private readonly CurrentWeatherParser currentWeatherParser;
        private readonly WeatherForecastParser weatherForecastParser;
        private readonly RadarParser radarParser;
        private readonly RadarImageCombiner radarImageCombiner;

        private const string AnalysisUrl = "https://opendata-download-metanalys.smhi.se/api/category/mesan1g/version/2/geotype/point/lon/15.605473/lat/56.187119/data.json";
        private const string ForecastUrl = "https://opendata-download-metfcst.smhi.se/api/category/pmp3g/version/2/geotype/point/lon/15.605473/lat/56.187119/data.json";
        private const string RadarUrl = "https://opendata-download-radar.smhi.se/api/version/latest/area/sweden/product/comp/";

        public SmhiFetcher(ILoggerFactory _loggerFactory, MinioConfiguration _minioConfiguration, IRedisCacheService _redis)
        {
            redis = _redis;
            logger = _loggerFactory.CreateLogger<SmhiFetcher>();

            radarParser = new RadarParser(_minioConfiguration, _redis, _loggerFactory);
            currentWeatherParser = new CurrentWeatherParser(_loggerFactory);
            weatherForecastParser = new WeatherForecastParser(_loggerFactory);
            radarImageCombiner = new RadarImageCombiner(_loggerFactory, _minioConfiguration, _redis);
        }

        public async Task Run()
        {
            var clock = SystemClock.Instance.GetCurrentInstant();
            var nowUtc = clock.InUtc();
            var url = $"{RadarUrl}{nowUtc.Year}/{nowUtc.Month:D2}/{nowUtc.Day:D2}?format=png";

            var radarResponseTask = RestRequestHandler.SendGetRequestAsync(url, radarParser, logger);
            var forecastResponseTask = RestRequestHandler.SendGetRequestAsync(ForecastUrl, weatherForecastParser, logger);
            var currentWeatherResponseTask = RestRequestHandler.SendGetRequestAsync(AnalysisUrl, currentWeatherParser, logger);

            await Task.WhenAll(radarResponseTask, forecastResponseTask, currentWeatherResponseTask);

            var radarResponse = radarResponseTask.Result;
            var forecastResponse = forecastResponseTask.Result;
            var currentWeatherResponse = currentWeatherResponseTask.Result;

            if (!radarResponse.Success || !forecastResponse.Success || !currentWeatherResponse.Success)
            {
                logger.LogWarning("Failed to fetch weather");
                return;
            }

            var radarFileLocation = await radarImageCombiner.GenerateRadar();

            var weatherReport = new WeatherReport()
            {
                CurrentWeather = ((CurrentWeatherResponse)currentWeatherResponse).CurrentWeather,
                WeatherForecast = ((WeatherForecastResponse)forecastResponse).WeatherForecast,
                RadarFileLocation = radarFileLocation
            };

            await redis.AddValue($"weather_report:{clock.ToUnixTimeSeconds()}", weatherReport);
        }
    }
}