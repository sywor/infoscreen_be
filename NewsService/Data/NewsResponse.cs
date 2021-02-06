using NewsService.Feedly;

namespace NewsService.Data
{
    public readonly struct NewsResponse
    {
        public NewsArticle NewsArticle { get; init; }
        public string Key { get; init; }
    }
}