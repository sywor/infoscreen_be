using System.Collections.Generic;

using Common.Response;

namespace WeatherService.Data
{
    public readonly struct RadarResponse : IResponse
    {
        public bool Success => true;
        public List<RadarImageResponse> RadarImages { get; init; }
    }
}