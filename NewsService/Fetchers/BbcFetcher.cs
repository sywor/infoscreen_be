using System.Linq;
using System.Text;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Fetchers.page;
using NewsService.Services;

using NodaTime;

namespace NewsService.Fetchers
{
    public class BbcFetcher : AbstractRssFetcher<BbcFetcher>
    {
        public const string NAME = "bbc";

        public BbcFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, RedisCacheService _redis, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _redis, _loggerFactory)
        {
            PageFetcher = DefaultPageFetcher.Create(_loggerFactory).Result;
        }

        protected override (bool success, ZonedDateTime value) ExtractPublishedAt(HtmlNodeCollection? _node, string _url, ArticleSourceType _articleSourceType)
        {
            var publishedAt = _node?.First().GetAttributeValue("datetime", null);

            if (publishedAt != null)
                return (true, ParseZonedDateTimeUTC(_node.First().InnerText));

            Logger.LogWarning($"Could not parse published at for article:: {{URL}}", _url);

            return (false, default);
        }

        protected override (bool success, string value) ExtractMedia(HtmlNodeCollection? _node, string _url, ArticleSourceType _articleSourceType)
        {
            var result = base.ExtractMedia(_node, _url, _articleSourceType);
            if (result.success)
            {
                return result;
            }

            if (_node == null)
            {
                Logger.LogWarning($"Image couldn't be found (possibly a video article?) for article: {{URL}}", _url);

                return (false, null)!;
            }

            var srcsetValue = _node?.First().GetAttributeValue("srcset", null);

            if (srcsetValue == null)
            {
                Logger.LogWarning($"Image srcset tag couldn't be found for article: {{URL}}", _url);

                return (false, null)!;
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

            if (source != null)
                return (true, source);

            Logger.LogWarning($"Image srcset value couldn't be found for article: {{URL}}", _url);

            return (false, null)!;
        }

        protected override (bool success, string value) ExtractBody(HtmlNodeCollection? _node, string _url, ArticleSourceType _articleSourceType)
        {
            if (_node == null)
            {
                Logger.LogWarning($"Body was empty for article: {{URL}}", _url);

                return (false, null)!;
            }

            var sb = new StringBuilder();

            foreach (var node in _node)
            {
                sb.Append(node.InnerText);

                if (sb.Length >= 500)
                    break;
            }

            return (true, sb.ToString());
        }
    }
}