using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Data;
using NewsService.Services;

using NodaTime;

using SimpleFeedReader;

namespace NewsService.Fetchers
{
    public abstract class AbstractRssFetcher<T> : AbstractFetcher<T>, IFetcher
    {
        protected AbstractRssFetcher(NewsSourceConfigurations _configuration, MinioConfiguration _minioConfiguration, string _name, ILoggerFactory _loggerFactory) : base(_configuration, _minioConfiguration, _name, _loggerFactory)
        {
        }

        public async Task<IEnumerable<PageResult>> Fetch(RedisCacheService _redis)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var fetchTime = now.InUtc();

            var reader = new FeedReader();
            var items = reader.RetrieveFeed(BaseUrl + LinkPage);

            var rssResponse = items != null
                ? items
                  .Where(_item => _item.Uri != null)
                  .Select(_item => new ArticleLinkResponse
                  {
                      Uri = _item.Uri.AbsoluteUri,
                      Title = _item.Title,
                      PublishedAt = OffsetDateTime.FromDateTimeOffset(_item.PublishDate).InFixedZone()
                  }).ToList()
                : new List<ArticleLinkResponse>();

            return await FetchAndParseArticle(fetchTime, rssResponse, _redis);
        }
    }
}