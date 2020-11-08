using NodaTime;

namespace NewsService.Data
{
    public struct NewsArticle
    {
        public static NewsArticle EMPTY => new NewsArticle();
        public string Title { get; set; }
        public string Source { get; set; }
        public Instant PublishedAt { get; set; }
        public string Content { get; set; }
        public byte[] Image { get; set; }
        public Instant FetchedAt { get; set; }

        public override string ToString()
        {
            return $"Title: {Title}, Source: {Source}";
        }
    }
}