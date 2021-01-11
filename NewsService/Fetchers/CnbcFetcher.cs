using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Fetchers.page;
using NewsService.Services;

using NodaTime;
using NodaTime.Text;
using NodaTime.TimeZones;

namespace NewsService.Fetchers
{
    public class CnbcFetcher : AbstractRssFetcher<CnbcFetcher>
    {
        public const string NAME = "cnbc";
        private readonly Regex publishedAtPattern = new Regex(@"Published [a-zA-Z]{3}, ([a-zA-Z]{3} \d{1,2} \d{4}).+(\d{1,2}:\d{1,2} [PA][M] EST)", RegexOptions.Compiled);
        private readonly DateTimeZoneCache dateTimeZoneProvider = new DateTimeZoneCache(TzdbDateTimeZoneSource.Default);

        public CnbcFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, RedisCacheService _redis, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _redis, _loggerFactory)
        {
            PageFetcher = DefaultPageFetcher.Create(_loggerFactory).Result;
        }

        protected override (bool success, ZonedDateTime value) ExtractPublishedAt(HtmlNodeCollection? _node, string _url)
        {
            if (_node == null)
            {
                Logger.LogWarning($"Published at was empty for article: {{URL}}", _url);

                return (false, default);
            }

            var html = _node.First().InnerHtml;
            var match = publishedAtPattern.Match(html);

            if (!match.Success)
            {
                Logger.LogWarning($"Published at didn't match pattern for article: {{URL}}", _url);

                return (false, default);
            }

            var date = match.Groups[1].Value;
            var time = match.Groups[2].Value;
            var dateTime = $"{date} {time}";

            var zonedDateTime = ZonedDateTimePattern
                                .CreateWithInvariantCulture("MMM d yyyy h:mm tt z", dateTimeZoneProvider)
                                .Parse(dateTime);

            if (zonedDateTime.Success)
                return (true, zonedDateTime.Value);

            Logger.LogWarning($"Published at didn't match pattern for article: {{URL}}", _url);

            return (false, default);
        }

        protected override (bool success, string value) ExtractBody(HtmlNodeCollection? _node, string _url)
        {
            if (_node == null)
            {
                Logger.LogWarning($"Body was null for article: {{URL}}", _url);

                return (false, null)!;
            }

            var sb = new StringBuilder();

            foreach (var node in _node)
            {
                sb.AppendLine(node.InnerText);
            }

            return (true, sb.ToString());
        }

        protected override (bool success, string value) ExtractImage(HtmlNodeCollection? _node, string _url)
        {
            var result = base.ExtractImage(_node, _url);
            if (result.success)
            {
                return result;
            }

            Logger.LogWarning($"Image couldn't be found (possibly a video article?) for article: {{URL}}", _url);

            return (false, null)!;
        }
    }
}