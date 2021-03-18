using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FrontendAPI.Config;
using FrontendAPI.Data;
using FrontendAPI.Extensions;

using Google.Protobuf.WellKnownTypes;

using Grpc.Net.Client;

using NewsService;

using NodaTime;
using NodaTime.Serialization.Protobuf;

using Duration = NodaTime.Duration;

namespace FrontendAPI.Services
{
    public class NewsGrpcService
    {
        private readonly NewsFetcher.NewsFetcherClient client;
        private readonly Duration updateInterval;
        private readonly List<NewsArticleResponse> articleBuffer = new();
        private Instant nextUpdate;
        private int articelIndex = 0;

        public NewsGrpcService(NewsGrpcServiceConfiguration _config)
        {
            var grpcChannel = GrpcChannel.ForAddress(_config.Host);
            updateInterval = Duration.FromMinutes(_config.UpdateInterval);
            nextUpdate = SystemClock.Instance.GetCurrentInstant();
            client = new NewsFetcher.NewsFetcherClient(grpcChannel);
        }

        public async Task<NewsArticleResponse> GetSingleArticle(string _articleKey)
        {
            var grpcResponse = client.GetArticleAsync(new ArticleRequest {ArticleKey = _articleKey});
            var article = (await grpcResponse.ResponseAsync).Article;

            return new NewsArticleResponse
            {
                Content = article.Content,
                Source = article.Source,
                Title = article.Title,
                Fetched = article.Fetched.ToInstant(),
                Image = article.Image.ToUrlString(),
                Published = article.Published.ToInstant(),
                Key = article.Key
            };
        }

        public async Task<List<NewsArticleResponse>> GetAllArticles()
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
                    Fetched = article.Fetched.ToInstant(),
                    Image = article.Image.ToUrlString(),
                    Published = article.Published.ToInstant(),
                    Key = article.Key
                });
            }
            return result;
        }

        public async Task<NewsArticleResponse> GetNextArticle()
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            if (now >= nextUpdate || !articleBuffer.Any())
            {
                articleBuffer.Clear();
                articleBuffer.AddRange(await GetAllArticles());
                articelIndex = 0;
                nextUpdate = now + updateInterval;
                articleBuffer.Sort((_r1, _r2) => _r2.Published.CompareTo(_r1.Published));
            }

            var response = articleBuffer[articelIndex];

            if (articelIndex >= articleBuffer.Count - 1)
            {
                articelIndex = 0;
            }
            else
            {
                articelIndex++;
            }

            return response;
        }
    }
}