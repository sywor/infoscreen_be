using System.Collections.Generic;

using Newtonsoft.Json;

using NodaTime;

namespace WeatherService.Json
{
    [JsonObject]
    public class JTimeSeries
    {
        [JsonProperty("validTime")]
        public Instant ValidTime { get; set; }

        [JsonProperty("parameters")]
        public List<JParameter> Parameters { get; set; }
    }
}