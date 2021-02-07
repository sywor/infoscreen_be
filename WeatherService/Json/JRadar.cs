using System.Collections.Generic;

using Newtonsoft.Json;

using NodaTime;

namespace WeatherService.Json
{
    [JsonObject]
    public class JRadar
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("updated")]
        public Instant Updated { get; set; }

        [JsonProperty("timeZone")]
        public string TimeZone { get; set; }

        [JsonProperty("files")]
        public List<JFile> Files { get; set; }
    }
}