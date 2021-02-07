using System;
using System.Collections.Generic;

using NodaTime;

namespace WeatherService.Data
{
    public readonly struct WeatherForecast
    {
        public Instant LeftDate { get; init; }
        public Instant RightDate { get; init; }
        public List<float> Temperatures { get; init; }
        public List<float> PrecipitationValues { get; init; }
        public List<float> WindSpeeds { get; init; }
        public List<int> WindDirection { get; init; }
        public List<int> WeatherSymbols { get; init; }
    }
}