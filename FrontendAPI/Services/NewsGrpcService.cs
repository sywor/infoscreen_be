using System.Collections.Generic;
using System.Threading.Tasks;

using FrontendAPI.Config;
using FrontendAPI.Extensions;

using Google.Protobuf.WellKnownTypes;

using Grpc.Net.Client;

using NewsService;

using NodaTime.Serialization.Protobuf;

namespace FrontendAPI.Services
{
    public class NewsGrpcService
    {
        private readonly NewsFetcher.NewsFetcherClient client;
        
        public NewsGrpcService(NewsGrpcServiceConfiguration _config)
        {
            var grpcChannel = GrpcChannel.ForAddress(_config.Host);
            client = new NewsFetcher.NewsFetcherClient(grpcChannel);
        }
        
        public async Task<List<NewsArticleResponse>> GetSingleArticle(string _articleKey)
        {
            var grpcResponse = client.GetArticleAsync(new ArticleRequest {ArticleKey = _articleKey});
            var article = (await grpcResponse.ResponseAsync).Article;

            var response = new NewsArticleResponse
            {
                Content = article.Content,
                Source = article.Source,
                Title = article.Title,
                Fetched = article.Fetched.ToInstant(),
                Image = article.Image.ToMinio(),
                Published = article.Published.ToInstant(),
                Key = article.Key
            };

            var result = new List<NewsArticleResponse>
            {
                response
            };

            return result;
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
                    Image = article.Image.ToMinio(),
                    Published = article.Published.ToInstant(),
                    Key = article.Key
                });
            }
            return result;
        }
    }
}