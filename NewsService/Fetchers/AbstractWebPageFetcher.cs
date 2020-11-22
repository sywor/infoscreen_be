using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsService.Data;
using NewsService.Services;
using NodaTime;

namespace NewsService.Fetchers
{
    public class AbstractWebPageFetcher<T> : AbstractFetcher<T>, IFetcher
    {
        public AbstractWebPageFetcher(IConfiguration _configuration, string _name, ILoggerFactory _loggerFactory) : base(_configuration, _name, _loggerFactory)
        {
            if (RootPageXPaths == null)
            {
                Logger.LogError("Invalid xpaths for root page: {Page}", _name);
                throw new ArgumentException($"Invalid xpaths for root page: {_name}");
            }
        }

        public async Task<IEnumerable<PageResult>> Fetch(RedisCacheService _redis)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var fetchTime = now.InUtc();

            var document = await FetchPage(BaseUrl + LinkPage);
            var rootNodeChildren = GetNodes(document, RootPageXPaths);

            var urls = (GetArticleLinksFromRootPage(rootNodeChildren) ?? Array.Empty<string>())
                .Select(_x => BaseUrl + _x);

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