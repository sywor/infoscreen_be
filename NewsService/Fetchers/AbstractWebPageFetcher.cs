using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Data;
using NewsService.Services;

using NodaTime;

namespace NewsService.Fetchers
{
    public class AbstractWebPageFetcher<T> : AbstractFetcher<T>, IFetcher
    {
        public AbstractWebPageFetcher(NewsSourceConfigurations _configuration, MinioConfiguration _minioConfiguration, string _name, ILoggerFactory _loggerFactory) : base(_configuration, _minioConfiguration, _name, _loggerFactory)
        {
        }

        public async Task<IEnumerable<PageResult>> Fetch(RedisCacheService _redis)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var fetchTime = now.InUtc();

            var document = await FetchPage(BaseUrl + LinkPage);

            if (document == null)
            {
                Logger.LogWarning("Could not fetch root page for {Name}", Name);

                return new List<PageResult>();
            }

            var rootNodeChildren = GetNodes(document, RootPageXPaths);

            var urls = (GetArticleLinksFromRootPage(rootNodeChildren) ?? Array.Empty<string>())
                       .Select(_x => new ArticleLinkResponse { Uri = BaseUrl + _x })
                       .ToList();

            return await FetchAndParseArticle(fetchTime, urls, _redis);
        }

        protected virtual IEnumerable<string>? GetArticleLinksFromRootPage(HtmlNodeCollection? _rootNodeChildren)
        {
            var urls = _rootNodeChildren?
                       .Select(_node => _node.GetAttributeValue("href", null))
                       .Where(_link => _link != null).ToList();

            return urls;
        }
    }
}