using Common.Response;

namespace WeatherService.Data
{
    public readonly struct WeatherForecastResponse : IResponse
    {
        public bool Success => true;
        public WeatherForecast WeatherForecast { get; init; }
    }
}