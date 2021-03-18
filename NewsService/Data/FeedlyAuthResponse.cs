using Common.Response;

using Newtonsoft.Json;

namespace NewsService.Data
{
    [JsonObject]
    public class FeedlyAuthResponse : IResponse
    {
        [JsonProperty("plan")]
        public string Plan { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonIgnore]
        public bool Success => true;
    }
}