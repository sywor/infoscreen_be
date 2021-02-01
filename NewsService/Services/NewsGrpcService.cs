using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.Extensions.Logging;

namespace NewsService.Services
{
    public class NewsGrpcService : NewsFetcher.NewsFetcherBase
    {
        private readonly NewsHandlerService newsHandler;
        private readonly ILogger logger;

        public NewsGrpcService(NewsHandlerService _handlerService, ILoggerFactory _loggerFactory)
        {
            newsHandler = _handlerService;
            logger = _loggerFactory.CreateLogger<NewsGrpcService>();
        }

        public override async Task GetAllArticles(Empty _request, IServerStreamWriter<ArticleResponse> _responseStream, ServerCallContext _context)
        {
            logger.LogInformation("All articles fetch request received");

            foreach (var newsArticleResponse in await newsHandler.NewsArticles())
            {
                if (!newsArticleResponse.Success)
                {
                    var statusMessage = CreateStatusMessage(newsArticleResponse.Message);
                    logger.LogInformation("Could not serv any articles, reason: {StatusMessage}", newsArticleResponse.Message);
                    await _responseStream.WriteAsync(statusMessage);
                    break;
                }

                var newsArticle = newsArticleResponse.NewsArticle;

                var article = new Article
                {
                    Key = newsArticleResponse.Key,
                    Title = newsArticle.Title,
                    ImagePath = newsArticle.ImageUrl,
                    Content = newsArticle.Content,
                    Source = newsArticle.Source,
                    FetchedUnix = newsArticle.FetchedAt,
                    PublishedUnix = newsArticle.PublishedAt
                };

                logger.LogInformation("Streaming article: {Title}", article.Title);

                await _responseStream.WriteAsync(new ArticleResponse {Article = article});
            }
        }

        public override async Task<ArticleResponse> GetArticle(ArticleRequest _request, ServerCallContext _context)
        {
            logger.LogInformation("Get next article request received");

            var newsResponse = await newsHandler.GetArticle(_request.ArticleKey);

            if (newsResponse.Success)
            {
                var newsArticle = newsResponse.NewsArticle;

                logger.LogInformation("Returning article: {Title}", newsArticle.Title);
                return new ArticleResponse
                {
                    Article = new Article
                    {
                        Key = _request.ArticleKey,
                        Title = newsArticle.Title,
                        ImagePath = newsArticle.ImageUrl,
                        Content = newsArticle.Content,
                        Source = newsArticle.Source,
                        FetchedUnix = newsArticle.FetchedAt,
                        PublishedUnix = newsArticle.PublishedAt
                    }
                };
            }

            var statusMessage = CreateStatusMessage(newsResponse.Message);
            logger.LogInformation("Could not serv article, reason: {StatusMessage}", newsResponse.Message);
            return statusMessage;
        }

        private static ArticleResponse CreateStatusMessage(string _message)
        {
            return new ArticleResponse {Status = new Status {Code = StatusCode.Failure, Message = _message}};
        }
    }
}