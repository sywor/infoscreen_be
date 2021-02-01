using System.Collections.Generic;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Grpc.Net.Client;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using NewsService;

namespace FrontendAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly ILogger<NewsController> logger;
        private readonly NewsFetcher.NewsFetcherClient client;

        public NewsController(ILogger<NewsController> _logger)
        {
            logger = _logger;

            var grpcChannel = GrpcChannel.ForAddress("https://localhost:5001");
            client = new NewsFetcher.NewsFetcherClient(grpcChannel);
        }

        [HttpGet]
        public async Task<List<NewsArticleResponse>> Get()
        {
            var response = client.GetAllArticles(new Empty());
            var stream = response.ResponseStream;
            var result = new List<NewsArticleResponse>();

            while (await stream.MoveNext(default))
            {
                var article = stream.Current.Article;
                result.Add(new NewsArticleResponse
                {
                    Content = article.Content,
                    Source = article.Source,
                    Title = article.Title,
                    FetchedUnix = article.FetchedUnix,
                    ImagePath = article.ImagePath,
                    PublishedUnix = article.PublishedUnix
                });
            }

            return result;
        }
    }
}