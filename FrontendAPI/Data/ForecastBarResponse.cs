using Common.Minio;

namespace FrontendAPI.Data
{
    public readonly struct ForecastBarResponse
    {
        public string WindDirectionIcon { get; init; }
        public string WeatherIcon { get; init; }
        public float Precipitation { get; init; }
        public float Temperature { get; init; }
        public float WindSpeed { get; init; }
    }
}