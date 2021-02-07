using System.Collections.Generic;

using Newtonsoft.Json;

using NodaTime;

namespace WeatherService.Json
{
    [JsonObject]
    public class JFile
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("valid")]
        public string Valid { get; set; }

        [JsonProperty("formats")]
        public List<JFormat> Formats { get; set; }
    }
}