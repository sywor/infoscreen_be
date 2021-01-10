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

        protected override bool ExtractPublishedAt(HtmlNodeCollection? _node, string _url, out ZonedDateTime _value)
        {
            if (_node == null)
            {
                LogAndSetFailure(_url, out _value);

                return false;
            }

            var srcValue = _node.First().InnerText;

            if (srcValue == null)
            {
                LogAndSetFailure(_url, out _value);

                return false;
            }

            srcValue = srcValue.Trim('\n').Trim();
            var now = SystemClock.Instance.GetCurrentInstant();

            if (Regex.IsMatch(srcValue, @"\d{1,2}m ago"))
            {
                var duration = DurationPatternMinutes.Parse(srcValue).Value;
                _value = new ZonedDateTime(now - duration, DateTimeZone.Utc);

                return true;
            }

            if (Regex.IsMatch(srcValue, @"\d{1,2}h ago"))
            {
                var duration = DurationPatternHours.Parse(srcValue).Value;
                _value = new ZonedDateTime(now - duration, DateTimeZone.Utc);

                return true;
            }

            var localDate = LocalDatePattern.Parse(srcValue).Value.AtStartOfDayInZone(DateTimeZone.Utc);
            _value = localDate;

            return true;
        }

        protected override bool ExtractImage(HtmlNodeCollection? _node, string _url, out string? _value)
        {
            if (!base.ExtractImage(_node, _url, out _value))
            {
                return false;
            }

            _value = _value?.Replace("&amp;", "&");

            return true;
        }
    }
}