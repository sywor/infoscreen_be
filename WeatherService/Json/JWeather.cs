using System.Collections.Generic;

using Newtonsoft.Json;

using NodaTime;

namespace WeatherService.Json
{
    [JsonObject]
    public class JWeather
    {
        [JsonProperty("approvedTime")]
        public Instant ApprovedTime { get; set; }
        
        [JsonProperty("referenceTime")]
        public Instant ReferenceTime { get; set; }

        [JsonProperty("timeSeries")]
        public List<JTimeSeries> TimeSeries { get; set; }
    }
}