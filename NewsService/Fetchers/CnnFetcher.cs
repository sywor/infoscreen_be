using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Services;

using NodaTime;

namespace NewsService.Fetchers
{
    public class CnnFetcher : AbstractRssFetcher<CnnFetcher>
    {
        public const string NAME = "cnn";

        private readonly Regex regex1 = new Regex(@".* (\d{1,2}[thst]{2} [A-Za-z]{3,8} \d{4})", RegexOptions.Compiled);
        private readonly Regex regex2 = new Regex(@".* (\d{4}).*\) (.*)", RegexOptions.Compiled);

        public CnnFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, RedisCacheService _redis, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _redis, _loggerFactory)
        {
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

            Match match = regex1.Match(srcValue);
            string dateTime;

            if (match.Success)
            {
                dateTime = match.Groups[1].Value;
            }
            else
            {
                match = regex2.Match(srcValue);

                if (!match.Success)
                {
                    LogAndSetFailure(_url, out _value);

                    return false;
                }

                dateTime = $"{match.Groups[1].Value} {match.Groups[2].Value}";
            }

            if (string.IsNullOrEmpty(dateTime))
            {
                LogAndSetFailure(_url, out _value);

                return false;
            }

            _value = ParseZonedDateTimeUTC(dateTime);

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
                HtmlNode realNode = node.Name.Equals("p") ? node.FirstChild : node;
                var text = Regex.Replace(realNode.InnerText, "<.*?>", string.Empty);
                text = text.Replace("(CNN) —", "").Trim('"').Trim();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(text).Append(" ");
                }
            }

            _value = sb.ToString();

            return true;
        }

        protected override bool ExtractImage(HtmlNodeCollection? _node, string _url, out string? _value)
        {
            if (!base.ExtractImage(_node, _url, out _value))
            {
                return false;
            }

            if (_value.StartsWith("data:image/gif"))
            {
                var value = _node?.First().Attributes["data-src"].Value;

                if (value == null)
                {
                    Logger.LogWarning($"Image src tag couldn't be found for article: {{URL}}", _url);
                    _value = null;

                    return false;
                }

                _value = value;
            }

            if (_value.StartsWith("//"))
            {
                _value = "https:" + _value;
            }

            return true;
        }
    }
}