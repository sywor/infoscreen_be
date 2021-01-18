using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Data;
using NewsService.Data.Parsers;
using NewsService.Fetchers.page;
using NewsService.Services;

using NodaTime;
using NodaTime.Text;

namespace NewsService.Fetchers
{
    public abstract class AbstractFetcher<T>
    {
        protected enum ArticleSourceType
        {
            RAW_ARTICLE,
            RENDERED_ARTICLE
        }

        public string Name { get; }

        private readonly RedisCacheService redis;
        private readonly FileDownloadParser fileDownloadParser;
        protected readonly ILogger Logger;
        protected readonly string BaseUrl;
        protected readonly string LinkPage;
        protected readonly string[] RootPageXPaths;
        protected readonly string[] TitleXPaths;
        protected readonly string[] ImageXPaths;
        protected readonly string[] BodyXPaths;
        protected readonly string[] PublishedAtXPaths;
        protected readonly List<InstantPattern> PublishedAtPatterns;
        protected IPageFetcher PageFetcher { get; set; }
        protected ILoggerFactory LoggerFactory { get; }

        protected AbstractFetcher(NewsSourceConfigurations _newsSourceConfigurations,
                                  MinioConfiguration _minioConfiguration,
                                  string _name,
                                  RedisCacheService _redis,
                                  ILoggerFactory _loggerFactory)
        {
            redis = _redis;
            LoggerFactory = _loggerFactory;
            Name = _name;
            Logger = _loggerFactory.CreateLogger<T>();

            fileDownloadParser = new FileDownloadParser(_minioConfiguration, _loggerFactory);

            var configuration = _newsSourceConfigurations[_name];

            BaseUrl = configuration.BaseUrl;
            LinkPage = configuration.LinkPage;

            PublishedAtPatterns = configuration.PublishedAtPattern
                                               .Select(InstantPattern.CreateWithInvariantCulture)
                                               .ToList();

            PublishedAtPatterns.Add(InstantPattern.General);

            var configurationXPaths = configuration.XPaths;
            RootPageXPaths = configurationXPaths.RootPage;
            TitleXPaths = configurationXPaths.Title;
            ImageXPaths = configurationXPaths.Image;
            BodyXPaths = configurationXPaths.Body;
            PublishedAtXPaths = configurationXPaths.PublishedAt;
        }

        protected async Task<List<PageResult>> FetchAndParseArticle(ZonedDateTime _fetchTime, List<ArticleLinkResponse> _articleLinkResponses)
        {
            var result = new List<PageResult>();

            if (!_articleLinkResponses.Any())
            {
                Logger.LogWarning("No responses received, skipping fetching for {Name}", Name);

                return result;
            }

            foreach (var articleLinkResponse in _articleLinkResponses)
            {
                var responseUri = articleLinkResponse.Uri;

                try
                {
                    if (!ShouldFetchArticle(articleLinkResponse))
                    {
                        Logger.LogInformation("{Url} for {Source} did not pass filter, skipping", responseUri, Name);

                        continue;
                    }

                    Logger.LogInformation("Fetching: {URL}", responseUri);

                    var articleSourceType = GetArticleSourceType(articleLinkResponse);

                    var document = articleSourceType switch
                    {
                        ArticleSourceType.RAW_ARTICLE      => await PageFetcher.FetchRawPage(responseUri),
                        ArticleSourceType.RENDERED_ARTICLE => await PageFetcher.FetchRenderedPage(responseUri),
                        _                                  => throw new ArgumentOutOfRangeException()
                    };

                    if (document == null)
                    {
                        continue;
                    }

                    string? title;

                    if (!string.IsNullOrEmpty(articleLinkResponse.Title))
                    {
                        title = articleLinkResponse.Title;
                    }
                    else
                    {
                        var titleNodes = GetNodes(document, TitleXPaths);
                        var (titleSuccess, titleValue) = ExtractTitle(titleNodes, responseUri, articleSourceType);

                        if (!titleSuccess)
                            continue;

                        title = titleValue;
                    }

                    var key = CreateRedisKey(title);

                    if (await redis.KeyExist(key))
                    {
                        Logger.LogInformation("Article with key: {Key} already exists, skipping", key);

                        continue;
                    }

                    ZonedDateTime publishedAt;

                    if (articleLinkResponse.PublishedAt != default)
                    {
                        publishedAt = articleLinkResponse.PublishedAt;
                    }
                    else
                    {
                        var publishedAtNodes = GetNodes(document, PublishedAtXPaths);
                        var (publishedAtSuccess, publishedAtValue) = ExtractPublishedAt(publishedAtNodes, responseUri, articleSourceType);

                        if (!publishedAtSuccess)
                            continue;

                        publishedAt = publishedAtValue;
                    }

                    var bodyNodes = GetNodes(document, BodyXPaths);
                    var (bodySuccess, body) = ExtractBody(bodyNodes, responseUri, articleSourceType);

                    if (!bodySuccess)
                        continue;


                    var imageNodes = GetNodes(document, ImageXPaths);
                    var (success, value) = ExtractMedia(imageNodes, responseUri, articleSourceType);

                    if (!success)
                        continue;

                    var imageResult = await RestRequestHandler.SendGetRequestAsync(value!, fileDownloadParser, Logger);

                    if (!imageResult.Success)
                    {
                        Logger.LogWarning("No image could be downloaded for article: {URL}", responseUri);

                        continue;
                    }

                    var imagePath = ((FileDownloadResponse) imageResult).FileUri;

                    var newsArticle = new NewsArticle
                    {
                        Title = title!,
                        Source = Name,
                        PublishedAt = publishedAt,
                        Body = body,
                        ImagePath = imagePath,
                        FetchedAt = _fetchTime,
                        Url = responseUri,
                        Type = GetArticleType(articleSourceType)
                    };

                    if (await redis.AddValue(key, newsArticle))
                    {
                        result.Add(new PageResult
                        {
                            ArticleKey = key,
                            PublishedAt = publishedAt,
                            FetchedAt = _fetchTime,
                            Source = Name
                        });
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Failed to fetch article for {Url}", responseUri);
                }
            }

            return result;
        }

        protected virtual bool ShouldFetchArticle(ArticleLinkResponse _url)
        {
            return true;
        }

        protected virtual ArticleSourceType GetArticleSourceType(ArticleLinkResponse _articleLinkResponse)
        {
            return ArticleSourceType.RENDERED_ARTICLE;
        }

        private string CreateRedisKey(string _title)
        {
            var keyTitle = _title.Trim();
            keyTitle = keyTitle.Replace("_", "");
            keyTitle = Regex.Replace(keyTitle, @"\s+", "_");
            keyTitle = Regex.Replace(keyTitle, @"[^\w\*]", "");

            var key = $"news_article:{Name}:{keyTitle}";

            return key;
        }

        protected static HtmlNodeCollection? GetNodes(HtmlDocument _page, IEnumerable<string> _xPaths)
        {
            return _xPaths
                   .Select(_xPath => _page.DocumentNode.SelectNodes(_xPath))
                   .FirstOrDefault(_tag => _tag != null);
        }

        protected virtual (bool success, string value) ExtractTitle(HtmlNodeCollection? _node, string _url, ArticleSourceType _articleSourceType)
        {
            if (!string.IsNullOrEmpty(_node?.First().InnerText))
                return (true, _node.First().InnerText)!;

            Logger.LogWarning("Title was empty for article: {URL}", _url);

            return (false, null)!;
        }

        protected virtual (bool success, ZonedDateTime value) ExtractPublishedAt(HtmlNodeCollection? _node, string _url, ArticleSourceType _articleSourceType)
        {
            if (!string.IsNullOrEmpty(_node?.First().InnerText))
                return (true, ParseZonedDateTimeUTC(_node.First().InnerText));

            Logger.LogWarning("Could not parse published at for article: {URL}", _url);

            return (false, default);
        }

        protected virtual (bool success, string value) ExtractBody(HtmlNodeCollection? _node, string _url, ArticleSourceType _articleSourceType)
        {
            if (!string.IsNullOrEmpty(_node?.First().InnerText))
                return (true, _node.First().InnerText);

            Logger.LogWarning("Body was empty for article: {URL}", _url);

            return (false, null)!;
        }

        protected virtual (bool success, string value) ExtractMedia(HtmlNodeCollection? _node, string _url, ArticleSourceType _articleSourceType)
        {
            var srcValue = _node?.First().GetAttributeValue("src", null);

            if (srcValue != null)
                return (true, srcValue);

            Logger.LogWarning("Image src tag couldn't be found for article: {URL}", _url);

            return (false, null)!;
        }

        protected virtual string GetArticleType(ArticleSourceType _articleSourceType)
        {
            return "TEXT";
        }

        protected virtual ZonedDateTime ParseZonedDateTimeUTC(string _dateTimeText)
        {
            return PublishedAtPatterns
                   .Select(_pattern => _pattern.Parse(_dateTimeText))
                   .First(_parseResult => _parseResult.Success)
                   .Value.InUtc();
        }
    }
}