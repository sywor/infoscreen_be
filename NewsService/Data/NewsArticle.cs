using Newtonsoft.Json;

using NodaTime;

namespace NewsService.Feedly
{
    public struct NewsArticle
    {
        public string OriginId { get; set; }
        public string Fingerprint { get; set; }
        public string Title { get; set; }
        public string Source { get; set; }
        public long PublishedAt { get; set; }
        public long FetchedAt { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string ArticleUrl { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}