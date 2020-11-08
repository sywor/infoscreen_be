namespace NewsService.Data
{
    public struct NewsResponse
    {
        public static NewsResponse FAILED(string _message = "") => new NewsResponse {Success = false, Message = _message};
        public static NewsResponse SUCCESS(NewsArticle _article) => new NewsResponse {Success = true, NewsArticle = _article};
        public NewsArticle NewsArticle { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}