using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace NewsService
{
    public class NewsService : NewsFetcher.NewsFetcherBase
    {
        public override Task GetAllArticles(Empty request, IServerStreamWriter<Article> responseStream, ServerCallContext context)
        {
            return base.GetAllArticles(request, responseStream, context);
        }

        public override Task<Article> GetNextArticle(Empty request, ServerCallContext context)
        {
            return base.GetNextArticle(request, context);
        }

        public override Task<Status> GetStatus(Empty request, ServerCallContext context)
        {
            return base.GetStatus(request, context);
        }
    }
}