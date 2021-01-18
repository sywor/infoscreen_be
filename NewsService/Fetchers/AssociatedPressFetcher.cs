using System.Linq;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Fetchers.page;
using NewsService.Services;

using NodaTime;

namespace NewsService.Fetchers
{
    public class AssociatedPressFetcher : AbstractWebPageFetcher<AssociatedPressFetcher>
    {
        public const string NAME = "associated_press";

        public AssociatedPressFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, RedisCacheService _redis, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _redis, _loggerFactory)
        {
            PageFetcher = DefaultPageFetcher.Create(_loggerFactory).Result;
        }

        protected override (bool success, ZonedDateTime value) ExtractPublishedAt(HtmlNodeCollection? _node, string _url, ArticleSourceType _articleSourceType)
        {
            if (_node == null)
            {
                Logger.LogWarning($"Could not parse published at for article: {{URL}}", _url);

                return (false, default);
            }

            var srcValue = _node.First().GetAttributeValue("data-source", null);

            if (srcValue != null)
                return (true, ParseZonedDateTimeUTC(srcValue));

            Logger.LogWarning($"Could not parse published at for article: {{URL}}", _url);

            return (false, default);
        }
    }
}