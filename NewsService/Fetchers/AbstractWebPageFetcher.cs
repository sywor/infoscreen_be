using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public abstract class AbstractWebPageFetcher<T> : AbstractFetcher<T>, IFetcher
    {
        public AbstractWebPageFetcher(NewsSourceConfigurations _configuration,
                                      MinioConfiguration _minioConfiguration,
                                      string _name,
                                      RedisCacheService _redis,
                                      ILoggerFactory _loggerFactory) : base(_configuration,
                                                                            _minioConfiguration,
                                                                            _name,
                                                                            _redis,
                                                                            _loggerFactory)
        {
        }

        public async Task<IEnumerable<PageResult>> Fetch()
        {
            var time = Stopwatch.StartNew();
            Logger.LogInformation("Fetching {Page}", Name);

            var now = SystemClock.Instance.GetCurrentInstant();
            var fetchTime = now.InUtc();

            var document = await PageFetcher.FetchRootPage(BaseUrl + LinkPage);

            if (document == null)
            {
                Logger.LogWarning("Could not fetch root page for {Name}", Name);

                return new List<PageResult>();
            }

            var rootNodeChildren = GetNodes(document, RootPageXPaths);

            var urls = (GetArticleLinksFromRootPage(rootNodeChildren) ?? Array.Empty<string>())
                       .Select(_x => new ArticleLinkResponse { Uri = _x.StartsWith("http") ? _x : BaseUrl + _x })
                       .ToList();

            var result = await FetchAndParseArticle(fetchTime, urls);
            Logger.LogInformation("{Page} Done fetching. Took: {Took} and fetched {Count} articles", Name, time.Elapsed, result.Count);

            return result;
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