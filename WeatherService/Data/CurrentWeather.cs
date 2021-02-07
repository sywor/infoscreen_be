using System.Collections.Generic;

using NodaTime;

namespace WeatherService.Data
{
    public readonly struct CurrentWeather
    {
        public Instant ReferenceTime { get; init; }
        public float Temperature { get; init; }
        public float WindSpeed { get; init; }
        public float WindGustSpeed { get; init; }
        public int WindDirectionSymbol { get; init; }
        public int Humidity { get; init; }
        public float Visibility { get; init; }
        public int CloudCover { get; init; }
        public int WeatherSymbol { get; init; }
        public List<float> Precipitation { get; init; }
    }
}