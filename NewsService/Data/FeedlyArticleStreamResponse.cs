using Common.Response;

using Newtonsoft.Json.Linq;

namespace NewsService.Data
{
    public class FeedlyArticleStreamResponse : IResponse
    {
        public JFeedlyArticleStream ArticleStream { get; set; }
        public bool Success => true;
    }
}