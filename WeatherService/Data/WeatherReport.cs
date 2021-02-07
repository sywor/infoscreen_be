namespace WeatherService.Data
{
    public readonly struct WeatherReport
    {
        public CurrentWeather CurrentWeather { get; init; }
        public WeatherForecast WeatherForecast { get; init; }
        public string RadarUrl { get; init; }
    }
}