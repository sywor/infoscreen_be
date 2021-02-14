using Common.Minio;

using Newtonsoft.Json;

using NodaTime;

namespace NewsService.Data
{
    public readonly struct NewsArticle
    {
        public string OriginId { get; init; }
        public string Fingerprint { get; init; }
        public string Title { get; init; }
        public string Source { get; init; }
        public Instant PublishedAt { get; init; }
        public Instant FetchedAt { get; init; }
        public string Content { get; init; }
        public MinioFile FileLocation { get; init; }
        public string ArticleUrl { get; init; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}