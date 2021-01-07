using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;
using NodaTime.TimeZones;

namespace NewsService.Fetchers
{
    public class CnbcFetcher : AbstractRssFetcher<CnbcFetcher>
    {
        private const string NAME = "cnbc";
        private readonly Regex publishedAtPattern = new Regex(@"Published [a-zA-Z]{3}, ([a-zA-Z]{3} \d{1,2} \d{4}).+(\d{1,2}:\d{1,2} [PA][M] EST)", RegexOptions.Compiled);
        private readonly DateTimeZoneCache dateTimeZoneProvider = new DateTimeZoneCache(TzdbDateTimeZoneSource.Default);

        public CnbcFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }

        protected override bool ExtractPublishedAt(HtmlNodeCollection? _node, string _url, out ZonedDateTime _value)
        {
            if (_node == null)
            {
                Logger.LogWarning($"Published at was empty for article: {{URL}}", _url);
                _value = default;
                return false;
            }

            var html = _node.First().InnerHtml;
            var match = publishedAtPattern.Match(html);

            if (!match.Success)
            {
                Logger.LogWarning($"Published at didn't match pattern for article: {{URL}}", _url);
                _value = default;
                return false;
            }

            var date = match.Groups[1].Value;
            var time = match.Groups[2].Value;
            var dateTime = $"{date} {time}";

            var zonedDateTime = ZonedDateTimePattern
                .CreateWithInvariantCulture("MMM d yyyy h:mm tt z", dateTimeZoneProvider)
                .Parse(dateTime);

            if (!zonedDateTime.Success)
            {
                Logger.LogWarning($"Published at didn't match pattern for article: {{URL}}", _url);
                _value = default;
                return false;
            }

            _value = zonedDateTime.Value;
            return true;
        }

        protected override bool ExtractBody(HtmlNodeCollection? _node, string _url, out string? _value)
        {
            if (_node == null)
            {
                Logger.LogWarning($"Body was null for article: {{URL}}", _url);
                _value = null;
                return false;
            }

            var sb = new StringBuilder();

            foreach (var node in _node)
            {
                sb.AppendLine(node.InnerText);
            }

            _value = sb.ToString();
            return true;
        }

        protected override bool ExtractImage(HtmlNodeCollection? _node, string _url, out string? _value)
        {
            if (base.ExtractImage(_node, _url, out _value)) 
                return true;

            Logger.LogWarning($"Image couldn't be found (possibly a video article?) for article: {{URL}}", _url);
            _value = null;
            return false;
        }
    }
}