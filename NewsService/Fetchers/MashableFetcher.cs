using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Data;
using NewsService.Fetchers.page;
using NewsService.Services;

using NodaTime;

namespace NewsService.Fetchers
{
    public class MashableFetcher : AbstractWebPageFetcher<MashableFetcher>
    {
        public const string NAME = "mashable";

        private readonly Regex urlRegex = new Regex(@".*mashable\.com\/(video).*");
        private readonly Regex imageRegex = new Regex(@".*url\(""(.*)""\).*");
        private readonly Regex timeRegex = new Regex(@"\D{3}, (.*) \+\d{4}");
        private readonly Regex videoRegex = new Regex(@".*(https:\/\/vdist\.aws\.mashable\.com.*?1080\.mp4)");

        public MashableFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, RedisCacheService _redis, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _redis, _loggerFactory)
        {
            PageFetcher = MashablePageFetcher.Create(_loggerFactory).Result;
        }

        protected override ArticleSourceType GetArticleSourceType(ArticleLinkResponse _articleLinkResponse)
        {
            return urlRegex.IsMatch(_articleLinkResponse.Uri) ? ArticleSourceType.RAW_ARTICLE : ArticleSourceType.RENDERED_ARTICLE;
        }

        protected override (bool success, ZonedDateTime value) ExtractPublishedAt(HtmlNodeCollection? _node, string _url, ArticleSourceType _articleSourceType)
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

        protected override (bool success, string value) ExtractTitle(HtmlNodeCollection? _node, string _url, ArticleSourceType _articleSourceType)
        {
            (bool, string) ExtractTitleRaw()
            {
                var (success, value) = base.ExtractTitle(_node, _url, _articleSourceType);

                return success ? (true, WebUtility.HtmlDecode(value)) : (false, null);
            }

            return _articleSourceType switch
            {
                ArticleSourceType.RAW_ARTICLE      => ExtractTitleRaw(),
                ArticleSourceType.RENDERED_ARTICLE => base.ExtractTitle(_node, _url, _articleSourceType),
                _                                  => throw new ArgumentOutOfRangeException(nameof(_articleSourceType), _articleSourceType, null)
            };
        }

        protected override (bool success, string value) ExtractBody(HtmlNodeCollection? _node, string _url, ArticleSourceType _articleSourceType)
        {
            (bool, string) ExtractBodyRaw()
            {
                if (!string.IsNullOrEmpty(_node?.First().InnerText))
                    return (true, _node.First().InnerText);

                Logger.LogInformation("Empty body for video article: {URL}", _url);

                return (true, string.Empty)!;
            }

            return _articleSourceType switch
            {
                ArticleSourceType.RAW_ARTICLE      => ExtractBodyRaw(),
                ArticleSourceType.RENDERED_ARTICLE => base.ExtractBody(_node, _url, _articleSourceType),
                _                                  => throw new ArgumentOutOfRangeException(nameof(_articleSourceType), _articleSourceType, null)
            };
        }

        protected override (bool success, string value) ExtractMedia(HtmlNodeCollection? _node, string _url, ArticleSourceType _articleSourceType)
        {
            return _articleSourceType switch
            {
                ArticleSourceType.RAW_ARTICLE      => ExtractVideo(_node, _url),
                ArticleSourceType.RENDERED_ARTICLE => ExtractImage(_node, _url),
                _                                  => throw new ArgumentOutOfRangeException(nameof(_articleSourceType), _articleSourceType, null)
            };
        }

        private (bool success, string value) ExtractVideo(HtmlNodeCollection? _node, string _url)
        {
            if (_node == null)
            {
                return (false, null)!;
            }

            Logger.LogInformation("Found video link in {URL}", _url);

            var match = videoRegex.Match(_node.First().InnerHtml);

            return match.Success ? (true, match.Groups[1].Value) : (false, null);
        }

        private (bool success, string value) ExtractImage(HtmlNodeCollection? _node, string _url)
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

            styleValue = WebUtility.HtmlDecode(styleValue);

            var match = imageRegex.Match(styleValue);
            if (match.Success)
            {
                return (true, match.Groups[1].Value);
            }

            Logger.LogWarning("Image source couldn't be found for article: {URL}", _url);

            return (false, null)!;
        }

        protected override string GetArticleType(ArticleSourceType _articleSourceType)
        {
            return _articleSourceType switch
            {
                ArticleSourceType.RAW_ARTICLE      => "VIDEO",
                ArticleSourceType.RENDERED_ARTICLE => "TEXT",
                _                                  => throw new ArgumentOutOfRangeException(nameof(_articleSourceType), _articleSourceType, null)
            };
        }
    }
}