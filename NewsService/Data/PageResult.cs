using NodaTime;

namespace NewsService.Data
{
    public struct PageResult
    {
        public string ArticleKey { get; set; }
        public ZonedDateTime PublishedAt { get; set; }
        public ZonedDateTime FetchedAt { get; set; }
        public string Source { get; set; }
    }
}