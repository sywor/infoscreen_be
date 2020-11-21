using NodaTime;

namespace NewsService.Data
{
    public struct NewsArticle
    {
        public static NewsArticle EMPTY => new NewsArticle();
        public string Title { get; set; }
        public string Source { get; set; }
        public ZonedDateTime PublishedAt { get; set; }
        public string Content { get; set; }
        public string ImagePath { get; set; }
        public ZonedDateTime FetchedAt { get; set; }
        public string Url { get; set; }

        public override string ToString()
        {
            return $"Title: {Title}, Source: {Source}";
        }
    }
}