using System.Linq;
using System.Text;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace NewsService.Fetchers
{
    public class BbcFetcher : AbstractRssFetcher<BbcFetcher>
    {
        private const string NAME = "bbc";

        public BbcFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }

        protected override bool ExtractPublishedAt(HtmlNodeCollection? _node, string _url, out ZonedDateTime _value)
        {
            var publishedAt = _node?.First().GetAttributeValue("datetime", null);

            if (publishedAt == null)
            {
                Logger.LogWarning($"Published at was empty for article: {{URL}}", _url);
                _value = default;
                return false;
            }

            _value = PublishedAtPattern
                .Parse(publishedAt)
                .Value.InUtc();

            return true;
        }

        protected override bool ExtractImage(HtmlNodeCollection? _node, string _url, out string? _value)
        {
            if (base.ExtractImage(_node, _url, out _value))
            {
                return true;
            }

            if (_node == null)
            {
                Logger.LogWarning($"Image couldn't be found (possibly a video article?) for article: {{URL}}", _url);
                _value = null;
                return false;
            }

            var srcsetValue = _node?.First().GetAttributeValue("srcset", null);

            if (srcsetValue == null)
            {
                Logger.LogWarning($"Image srcset tag couldn't be found for article: {{URL}}", _url);
                _value = null;
                return false;
            }

            var source = srcsetValue.Split(',')
                .Select(_str =>
                {
                    var tmp = _str.Split(' ');
                    return new
                    {
                        Key = int.Parse(tmp[1].TrimEnd('w')),
                        Value = tmp[0]
                    };
                })
                .OrderByDescending(_key => _key.Key)
                .Select(_obj => _obj.Value)
                .FirstOrDefault();

            if (source == null)
            {
                Logger.LogWarning($"Image srcset value couldn't be found for article: {{URL}}", _url);
                _value = null;
                return false;
            }

            _value = source;
            return true;
        }

        protected override bool ExtractBody(HtmlNodeCollection? _node, string _url, out string? _value)
        {
            if (_node == null)
            {
                Logger.LogWarning($"Body was empty for article: {{URL}}", _url);
                _value = null;
                return false;
            }

            var sb = new StringBuilder();

            foreach (var node in _node)
            {
                sb.Append(node.InnerText);
                if (sb.Length >= 500)
                    break;
            }

            _value = sb.ToString();
            return true;
        }
    }
}