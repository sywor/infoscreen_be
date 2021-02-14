using Common.Minio;

namespace FrontendAPI.Data
{
    public readonly struct WeatherReportResponse
    {
        public MinioFile WeatherIcon { get; init; }
        public string WeatherDescription { get; init; }
        public float Temperature { get; init; }
        public float WindSpeed { get; init; }
        public float WindGustSpeed { get; init; }
        public MinioFile WindDirection { get; init; }
        public MinioFile RadarImage { get; init; }
        public PrecipitationResponse PrecipitationValue { get; init; }
        public int Humidity { get; init; }
        public float Visibility { get; init; }
        public int CloudCover { get; init; }
        public WeatherForecastResponse WeatherForecast { get; init; }
    }
}