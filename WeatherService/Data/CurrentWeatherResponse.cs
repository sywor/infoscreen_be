using Common.Response;

namespace WeatherService.Data
{
    public readonly struct CurrentWeatherResponse : IResponse
    {
        public bool Success => true;
        public CurrentWeather CurrentWeather { get; init; }
    }
}