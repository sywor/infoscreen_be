using Common.Minio;

using NodaTime;

namespace WeatherService.Data
{
    public readonly struct WeatherReport
    {
        public CurrentWeather CurrentWeather { get; init; }
        public WeatherForecast WeatherForecast { get; init; }
        public MinioFile RadarFileLocation { get; init; }
        public ZonedDateTime ProducedAt { get; init; }
    }
}