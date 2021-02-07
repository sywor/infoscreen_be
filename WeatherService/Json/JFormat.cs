using Newtonsoft.Json;

using NodaTime;

namespace WeatherService.Json
{
    [JsonObject]
    public class JFormat
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("updated")]
        public Instant Updated { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }
    }
}