using System;
using System.Collections.Generic;

using NodaTime;

namespace WeatherService.Data
{
    public readonly struct WeatherForecast
    {
        public Instant StartDate { get; init; }
        public Instant EndDate { get; init; }
        public List<WeatherForecastBar> WeatherForecastBars { get; init; }
        public float MaxTemp { get; init; }
        public float MinTemp { get; init; }
        public float MaxWindSpeed { get; init; }
        public float MinWindSpeed { get; init; }
    }
}