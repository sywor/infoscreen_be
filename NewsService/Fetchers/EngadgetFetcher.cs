using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Fetchers.page;
using NewsService.Services;

using NodaTime;
using NodaTime.Text;

namespace NewsService.Fetchers
{
    public class EngadgetFetcher : AbstractWebPageFetcher<EngadgetFetcher>
    {
        public const string NAME = "engadget";
        private static readonly DurationPattern DurationPatternMinutes = DurationPattern.Create("m'm ago'", CultureInfo.InvariantCulture);
        private static readonly DurationPattern DurationPatternHours = DurationPattern.Create("m'h ago'", CultureInfo.InvariantCulture);
        private static readonly LocalDatePattern LocalDatePattern = LocalDatePattern.Create("MMMM d, yyyy", CultureInfo.InvariantCulture);

        public EngadgetFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, RedisCacheService _redis, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _redis, _loggerFactory)
        {
            PageFetcher = EngadgetPageFetcher.Create(_loggerFactory).Result;
        }

        protected override (bool success, ZonedDateTime value) ExtractPublishedAt(HtmlNodeCollection? _node, string _url)
        {
            if (_node == null)
            {
                Logger.LogWarning("Could not parse published at for article: {URL}", _url);

                return (false, default);
            }

            var srcValue = _node.First().InnerText;

            if (srcValue == null)
            {
                Logger.LogWarning("Could not parse published at for article: {URL}", _url);

                return (false, default);
            }

            srcValue = srcValue.Trim('\n').Trim();
            var now = SystemClock.Instance.GetCurrentInstant();

            if (Regex.IsMatch(srcValue, @"\d{1,2}m ago"))
            {
                var duration = DurationPatternMinutes.Parse(srcValue).Value;

                return (true, new ZonedDateTime(now - duration, DateTimeZone.Utc));
            }

            if (Regex.IsMatch(srcValue, @"\d{1,2}h ago"))
            {
                var duration = DurationPatternHours.Parse(srcValue).Value;

                return (true, new ZonedDateTime(now - duration, DateTimeZone.Utc));
            }

            var localDate = LocalDatePattern.Parse(srcValue).Value.AtStartOfDayInZone(DateTimeZone.Utc);

            return (true, localDate);
        }

        protected override (bool success, string value) ExtractImage(HtmlNodeCollection? _node, string _url)
        {
            var result = base.ExtractImage(_node, _url);
            if (result.success)
            {
                return result;
            }

            result.value = result.value.Replace("&amp;", "&");

            return result;
        }
    }
}