using System.Linq;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Fetchers.page;
using NewsService.Services;

namespace NewsService.Fetchers
{
    public class ArsTechnicaFetcher : AbstractRssFetcher<ArsTechnicaFetcher>
    {
        public const string NAME = "arstechnica";
        private readonly Regex regex = new Regex(@".*'(http.+)'.*", RegexOptions.Compiled);

        public ArsTechnicaFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, RedisCacheService _redis, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _redis, _loggerFactory)
        {
            PageFetcher = DefaultPageFetcher.Create(_loggerFactory).Result;
        }

        protected override (bool success, string value) ExtractMedia(HtmlNodeCollection? _node, string _url, ArticleSourceType _articleSourceType)
        {
            var result = base.ExtractMedia(_node, _url, _articleSourceType);
            if (result.success)
            {
                return result;
            }

            Logger.LogWarning("Attempting fallback method");
            var srcValue = _node?.First().GetAttributeValue("style", null);

            if (srcValue == null)
            {
                Logger.LogWarning($"Image style tag couldn't be found for article: {{URL}}", _url);

                return (false, null)!;
            }

            Match match = regex.Match(srcValue);

            if (match.Success)
                return (true, match.Groups[1].Value);

            Logger.LogWarning($"Image style tag didn't match pattern for article: {{URL}}", _url);

            return (false, null)!;
        }

        protected override (bool success, string value) ExtractBody(HtmlNodeCollection? _node, string _url, ArticleSourceType _articleSourceType)
        {
            if (_node == null)
            {
                Logger.LogWarning($"Body could be found for article: {{URL}}", _url);

                return (false, null)!;
            }

            var result = _node.FirstOrDefault(_x => !string.IsNullOrEmpty(_x.InnerText))?.InnerText;

            if (result != null)
                return (true, result);

            Logger.LogWarning($"Body was empty for article: {{URL}}", _url);

            return (false, null)!;
        }
    }
}