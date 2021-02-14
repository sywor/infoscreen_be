using System.Collections.Generic;

using NodaTime;

namespace FrontendAPI.Data
{
    public readonly struct WeatherForecastResponse
    {
        public float MinTemp { get; init; }
        public float MaxTemp { get; init; }
        public float MinWindSpeed { get; init; }
        public float MaxWindSpeed { get; init; }
        public Instant StartTime { get; init; }
        public Instant EndTime { get; init; }
        public List<ForecastBarResponse> Forecast { get; init; }
    }
}