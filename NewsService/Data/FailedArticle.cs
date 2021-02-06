namespace NewsService.Data
{
    public readonly struct FailedArticle
    {
        public string Reason { get; init; }
        public long FetchedAt { get; init; }
    }
}