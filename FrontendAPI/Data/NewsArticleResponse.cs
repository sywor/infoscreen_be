using Common.Minio;

using NodaTime;

namespace FrontendAPI.Data
{
    public readonly struct NewsArticleResponse
    {
        public string Title { get; init; }
        public Instant Fetched { get; init; }
        public Instant Published { get; init; }
        public string Image { get; init; }
        public string Content { get; init; }
        public string Source { get; init; }
        public string Key { get; init; }
    }
}