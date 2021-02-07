using NodaTime;

namespace WeatherService.Data
{
    public readonly struct RadarImageResponse
    {
        public Instant TimeStamp { get; init; }
        public string OriginUrl { get; init; }
        public string Key { get; init; }
        public string ImageUrl { get; init; }
    }
}