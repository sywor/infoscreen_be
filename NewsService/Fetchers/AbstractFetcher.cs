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

        protected AbstractFetcher(NewsSourceConfigurations _newsSourceConfigurations,
                                  MinioConfiguration _minioConfiguration,
                                  string _name,
                                  RedisCacheService _redis,
                                  ILoggerFactory _loggerFactory)
        {
            redis = _redis;
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

                    var document = await PageFetcher.FetchPage(responseUri);

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

                        if (!ExtractTitle(titleNodes, responseUri, out title))
                            continue;
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

                        if (!ExtractPublishedAt(publishedAtNodes, responseUri, out publishedAt))
                            continue;
                    }

                    var bodyNodes = GetNodes(document, BodyXPaths);

                    if (!ExtractBody(bodyNodes, responseUri, out var body))
                        continue;

                    var imageNodes = GetNodes(document, ImageXPaths);

                    if (!ExtractImage(imageNodes, responseUri, out var imageUrl))
                        continue;

                    var imageResult = await RestRequestHandler.SendGetRequestAsync(imageUrl!, fileDownloadParser, Logger);

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
                        Body = body!,
                        ImagePath = imagePath,
                        FetchedAt = _fetchTime,
                        Url = responseUri
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

        private string CreateRedisKey(string _title)
        {
            var keyTitle = Regex.Replace(_title!.Trim(), @"\s+", "_");
            keyTitle = Regex.Replace(keyTitle, @"[^\w\*]", "");

            var key = $"news_article:{Name}:{keyTitle}";

            return key;
        }

        protected static HtmlNodeCollection? GetNodes(HtmlDocument _page, string[] _xPaths)
        {
            return _xPaths
                   .Select(_xPath => _page.DocumentNode.SelectNodes(_xPath))
                   .FirstOrDefault(_tag => _tag != null);
        }

        protected virtual bool ExtractTitle(HtmlNodeCollection? _node, string _url, out string? _value)
        {
            if (string.IsNullOrEmpty(_node?.First().InnerText))
            {
                Logger.LogWarning("Title was empty for article: {URL}", _url);
                _value = null;

                return false;
            }

            _value = _node.First().InnerText;

            return true;
        }

        protected virtual bool ExtractPublishedAt(HtmlNodeCollection? _node, string _url, out ZonedDateTime _value)
        {
            if (string.IsNullOrEmpty(_node?.First().InnerText))
            {
                Logger.LogWarning("Could not parse published at for article: {URL}", _url);
                _value = default;

                return false;
            }

            _value = ParseZonedDateTimeUTC(_node.First().InnerText);

            return true;
        }

        protected virtual ZonedDateTime ParseZonedDateTimeUTC(string _dateTimeText)
        {
            return PublishedAtPatterns
                   .Select(_pattern => _pattern.Parse(_dateTimeText))
                   .First(_parseResult => _parseResult.Success)
                   .Value.InUtc();
        }

        protected virtual bool ExtractBody(HtmlNodeCollection? _node, string _url, out string? _value)
        {
            if (string.IsNullOrEmpty(_node?.First().InnerText))
            {
                Logger.LogWarning("Body was empty for article: {URL}", _url);
                _value = null;

                return false;
            }

            _value = _node.First().InnerText;

            return true;
        }

        protected virtual bool ExtractImage(HtmlNodeCollection? _node, string _url, out string? _value)
        {
            var srcValue = _node?.First().GetAttributeValue("src", null);

            if (srcValue == null)
            {
                Logger.LogWarning("Image src tag couldn't be found for article: {URL}", _url);
                _value = null;

                return false;
            }

            _value = srcValue;

            return true;
        }

        protected virtual bool ShouldFetchArticle(ArticleLinkResponse _url)
        {
            return true;
        }

        protected void LogAndSetFailure<Type>(string _url, out Type _value)
        {
            Logger.LogWarning("Could not parse {Type} for article: {URL}", nameof(Type), _url);
            _value = default!;
        }
    }
}