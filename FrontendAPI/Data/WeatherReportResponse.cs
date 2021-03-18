using Common.Minio;

namespace FrontendAPI.Data
{
    public readonly struct WeatherReportResponse
    {
        public string WeatherIcon { get; init; }
        public string WeatherDescription { get; init; }
        public float Temperature { get; init; }
        public float WindSpeed { get; init; }
        public float WindGustSpeed { get; init; }
        public string WindDirection { get; init; }
        public string RadarImage { get; init; }
        public PrecipitationResponse PrecipitationValue { get; init; }
        public int Humidity { get; init; }
        public float Visibility { get; init; }
        public int CloudCover { get; init; }
        public WeatherForecastResponse WeatherForecast { get; init; }
    }
}