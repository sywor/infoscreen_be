using NodaTime;

namespace NewsService.Data
{
    public readonly struct FailedArticle
    {
        public string Reason { get; init; }
        public Instant FetchedAt { get; init; }
    }
}