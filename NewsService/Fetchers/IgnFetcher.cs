using System.Linq;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Data;
using NewsService.Fetchers.page;
using NewsService.Services;

namespace NewsService.Fetchers
{
    public class IgnFetcher : AbstractRssFetcher<IgnFetcher>
    {
        public const string NAME = "ign";

        private readonly Regex urlFilter = new Regex(@".*www\.ign\.com\/(?!videos|slideshows).*");
        private readonly Regex iframeFilter = new Regex(@".*url\(""(.*)""\).*");

        public IgnFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, RedisCacheService _redis, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _redis, _loggerFactory)
        {
            PageFetcher = DefaultPageFetcher.Create(_loggerFactory).Result;
        }

        protected override bool ShouldFetchArticle(ArticleLinkResponse _url)
        {
            return urlFilter.IsMatch(_url.Uri);
        }

        protected override bool ExtractImage(HtmlNodeCollection? _node, string _url, out string? _value)
        {
            var htmlNode = _node?.First();

            if (htmlNode == null)
            {
                Logger.LogWarning("Image src tag couldn't be found for article: {URL}", _url);
                _value = null;

                return false;
            }

            if (htmlNode.Name == "iframe")
            {
                Logger.LogInformation("Found iFrame, using alternative method for image fetching");
                var src = htmlNode.GetAttributeValue("data-src", null);
                var url = src.StartsWith("http") ? src : $"https://nordic.ign.com{src}";

                Logger.LogInformation("iFrame source {URL}", url);

                var fetchPage = PageFetcher.FetchPage(url).Result;

                var selectSingleNode = fetchPage?.DocumentNode.SelectSingleNode("//div[@class='video-embed']//div[@class='screen-background-image']");
                var attributeValue = selectSingleNode?.GetAttributeValue("style", null);
                attributeValue = attributeValue?.Replace("&quot;", "\"");

                var match = iframeFilter.Match(attributeValue ?? string.Empty);
                if (match.Success)
                {
                    _value = match.Groups[1].Value;

                    return true;
                }
                Logger.LogWarning("Image src tag couldn't be found for article: {URL}", _url);
                _value = null;

                return false;
            }

            var url0 = htmlNode.GetAttributeValue("data-thumb-src", null);
            var url1 = htmlNode.GetAttributeValue("data-src", null);
            var url2 = htmlNode.GetAttributeValue("src", null);
            var url4 = htmlNode.GetAttributeValue("href", null);

            _value = url0 ?? url1 ?? url2 ?? url4;

            return true;
        }

        protected override bool ExtractBody(HtmlNodeCollection? _node, string _url, out string? _value)
        {
            if (!base.ExtractBody(_node, _url, out _value))
            {
                return false;
            }

            _value = Regex.Replace(_value, "<.*?>", string.Empty);

            return true;
        }
    }
}