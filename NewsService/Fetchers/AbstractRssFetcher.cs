using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsService.Data;
using NewsService.Services;
using NodaTime;
using SimpleFeedReader;

namespace NewsService.Fetchers
{
    public abstract class AbstractRssFetcher<T> : AbstractFetcher<T>, IFetcher
    {
        protected AbstractRssFetcher(IConfiguration _configuration, string _name, ILoggerFactory _loggerFactory) : base(_configuration, _name, _loggerFactory)
        {
        }

        public async Task<IEnumerable<PageResult>> Fetch(RedisCacheService _redis)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var fetchTime = now.InUtc();

            var reader = new FeedReader();
            var items = reader.RetrieveFeed(BaseUrl + LinkPage);
            var urls = items.Select(_item => _item.Uri.AbsoluteUri).ToList();

            return await FetchAndParseArticle(fetchTime, urls, _redis);
        }
    }
}