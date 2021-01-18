using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Fetchers.page;
using NewsService.Services;

using NodaTime;

using PuppeteerSharp;

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
            PageFetcher = DefaultPageFetcher.Create(_loggerFactory, WaitUntilNavigation.Networkidle2).Result;
        }
        
        protected override (bool success, ZonedDateTime value) ExtractPublishedAt(HtmlNodeCollection? _node, string _url, ArticleSourceType _articleSourceType)
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
                    Logger.LogWarning("Could not parse published at for article: {URL}", _url);

                    return (false, default);
                }

                dateTime = $"{match.Groups[1].Value} {match.Groups[2].Value}";
            }

            if (!string.IsNullOrEmpty(dateTime))
                return (true, ParseZonedDateTimeUTC(dateTime));

            Logger.LogWarning("Could not parse published at for article: {URL}", _url);

            return (false, default);
        }

        protected override (bool success, string value) ExtractBody(HtmlNodeCollection? _node, string _url, ArticleSourceType _articleSourceType)
        {
            if (_node == null)
            {
                Logger.LogWarning($"Body was null for article: {{URL}}", _url);

                return (false, null)!;
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

            return (true, sb.ToString());
        }

        protected override (bool success, string value) ExtractMedia(HtmlNodeCollection? _node, string _url, ArticleSourceType _articleSourceType)
        {
            var result = base.ExtractMedia(_node, _url, _articleSourceType);
            if (result.success)
            {
                return result;
            }

            if (result.value.StartsWith("data:image/gif"))
            {
                var value = _node?.First().Attributes["data-src"].Value;

                if (value == null)
                {
                    Logger.LogWarning($"Image src tag couldn't be found for article: {{URL}}", _url);

                    return (false, null)!;
                }

                result.value = value;
            }

            if (result.value.StartsWith("//"))
            {
                result.value = "https:" + result.value;
            }

            return result;
        }
    }
}