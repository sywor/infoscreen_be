namespace FrontendAPI
{
    public readonly struct NewsArticleResponse
    {
        public string Title { get; init; }
        public long FetchedUnix { get; init; }
        public long PublishedUnix { get; init; }
        public string ImagePath { get; init; }
        public string Content { get; init; }
        public string Source { get; init; }
        public string Key { get; init; }
    }
}