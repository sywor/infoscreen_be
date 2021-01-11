using System.Linq;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Data;
using NewsService.Fetchers.page;
using NewsService.Services;

using NodaTime;
using NodaTime.Text;

namespace NewsService.Fetchers
{
    public class MashableFetcher : AbstractWebPageFetcher<MashableFetcher>
    {
        public const string NAME = "mashable";

        private readonly Regex urlRegex = new Regex(@".*mashable\.com\/(?!video).*");
        private readonly Regex imageRegex = new Regex(@".*url\(""(.*)""\).*");
        private readonly Regex timeRegex = new Regex(@"\D{3}, (.*) \+\d{4}");

        public MashableFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, RedisCacheService _redis, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _redis, _loggerFactory)
        {
            PageFetcher = MashablePageFetcher.Create(_loggerFactory).Result;
        }

        protected override bool ShouldFetchArticle(ArticleLinkResponse _url)
        {
            return urlRegex.IsMatch(_url.Uri);
        }

        protected override (bool success, ZonedDateTime value) ExtractPublishedAt(HtmlNodeCollection? _node, string _url)
        {
            var time = _node?.First().GetAttributeValue("datetime", null);

            if (string.IsNullOrEmpty(time))
            {
                Logger.LogWarning("Could not parse published at for article: {URL}", _url);

                return (false, default);
            }

            var match = timeRegex.Match(time);

            if (match.Success)
            {
                return (true, ParseZonedDateTimeUTC(match.Groups[1].Value));
            }

            Logger.LogWarning("Could not parse published at for article: {URL}", _url);

            return (false, default);
        }

        protected override (bool success, string value) ExtractImage(HtmlNodeCollection? _node, string _url)
        {
            var srcValue = _node?.First().GetAttributeValue("src", null);

            if (srcValue != null)
                return (true, srcValue);

            var styleValue = _node?.First().GetAttributeValue("style", null);

            if (styleValue == null)
            {
                Logger.LogWarning("Image source couldn't be found for article: {URL}", _url);

                return (false, null)!;
            }

            styleValue = styleValue.Replace("&quot;", "\"");

            var match = imageRegex.Match(styleValue);
            if (match.Success)
            {
                return (true, match.Groups[1].Value);
            }

            Logger.LogWarning("Image source couldn't be found for article: {URL}", _url);

            return (false, null)!;
        }
    }
}