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

        protected override bool ExtractPublishedAt(HtmlNodeCollection? _node, string _url, out ZonedDateTime _value)
        {
            if (_node == null)
            {
                Logger.LogWarning($"Could not parse published at for article: {{URL}}", _url);
                _value = default;

                return false;
            }

            var srcValue = _node.First().GetAttributeValue("data-source", null);

            if (srcValue == null)
            {
                Logger.LogWarning($"Could not parse published at for article: {{URL}}", _url);
                _value = default;

                return false;
            }

            _value = ParseZonedDateTimeUTC(srcValue);

            return true;
        }
    }
}