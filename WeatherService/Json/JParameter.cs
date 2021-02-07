using System.Collections.Generic;

using Newtonsoft.Json;

namespace WeatherService.Json
{
    [JsonObject]
    public class JParameter
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("levelType")]
        public string LevelType { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }

        [JsonProperty("values")]
        public List<float> Values { get; set; }
    }
}