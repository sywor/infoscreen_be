using System.Linq;
using System.Text;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using NewsService.Config;

using NodaTime;

namespace NewsService.Fetchers
{
    public class BbcFetcher : AbstractRssFetcher<BbcFetcher>
    {
        private const string NAME = "bbc";

        public BbcFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _loggerFactory)
        {
        }

        protected override bool ExtractPublishedAt(HtmlNodeCollection? _node, string _url, out ZonedDateTime _value)
        {
            var publishedAt = _node?.First().GetAttributeValue("datetime", null);

            if (publishedAt == null)
            {
                Logger.LogWarning($"Could not parse published at for article:: {{URL}}", _url);
                _value = default;

                return false;
            }

            _value = ParseDateTime(_node.First().InnerText);

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