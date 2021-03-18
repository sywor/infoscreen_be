using NodaTime;

namespace NewsService.Data
{
    public class FeedlyTokens
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public ZonedDateTime Expires { get; set; }
    }
}