using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsService.Data;
using NewsService.Data.Parsers;
using NewsService.Services;
using NodaTime;
using NodaTime.Text;
using PuppeteerSharp;

namespace NewsService.Fetchers
{
    public abstract class AbstractFetcher<T>
    {
        protected string Name { get; }

        private readonly FileDownloadParser fileDownloadParser;
        protected readonly ILogger Logger;
        protected readonly string BaseUrl;
        protected readonly string LinkPage;
        protected readonly string[] RootPageXPaths;
        protected readonly string[] TitleXPaths;
        protected readonly string[] ImageXPaths;
        protected readonly string[] BodyXPaths;
        protected readonly string[] PublishedAtXPaths;
        protected readonly InstantPattern PublishedAtPattern;

        protected AbstractFetcher(IConfiguration _configuration, string _name, ILoggerFactory _loggerFactory)
        {
            Name = _name;
            Logger = _loggerFactory.CreateLogger<T>();
            fileDownloadParser = new FileDownloadParser(_configuration, _loggerFactory);

            var configuration = _configuration.GetSection("NewsSources")
                .GetChildren()
                .SingleOrDefault(_x => _x["Name"].Equals(_name));

            if (configuration == null)
            {
                Logger.LogError("Configuration cannot be null: {Page}", _name);
                throw new ArgumentException();
            }

            if (configuration["BaseUrl"] == null)
            {
                Logger.LogError("Base url cannot be null: {Page}", _name);
                throw new ArgumentException($"Base url cannot be null: {_name}");
            }

            BaseUrl = configuration["BaseUrl"];

            if (configuration["LinkPage"] == null)
            {
                Logger.LogError("Url cannot be null: {Page}", _name);
                throw new ArgumentException($"Url cannot be null: {_name}");
            }

            LinkPage = configuration["LinkPage"];

            var publishedAtConfig = configuration["PublishedAtPattern"];

            if (publishedAtConfig == null)
            {
                Logger.LogError("Published at cannot be null: {Page}", _name);
                throw new ArgumentException($"Published at cannot be null: {_name}");
            }

            PublishedAtPattern = publishedAtConfig == "ISO_8601"
                ? InstantPattern.General
                : InstantPattern.CreateWithInvariantCulture(publishedAtConfig);

            foreach (var xpath in configuration.GetSection("XPaths").GetChildren())
            {
                switch (xpath.Key)
                {
                    case "RootPage":
                        RootPageXPaths = xpath.Get<string[]>();
                        break;
                    case "Title":
                        TitleXPaths = xpath.Get<string[]>();
                        break;
                    case "Image":
                        ImageXPaths = xpath.Get<string[]>();
                        break;
                    case "Body":
                        BodyXPaths = xpath.Get<string[]>();
                        break;
                    case "PublishedAt":
                        PublishedAtXPaths = xpath.Get<string[]>();
                        break;
                    default:
                        throw new ArgumentException($"XPaths cannot be null: {_name}");
                }
            }

            if (TitleXPaths == null || ImageXPaths == null || BodyXPaths == null)
            {
                Logger.LogError("Invalid xpaths for page: {Page}", _name);
                throw new ArgumentException($"Invalid xpaths for page: {_name}");
            }
        }

        protected async Task<List<PageResult>> FetchAndParseArticle(ZonedDateTime _fetchTime, List<ArticleLinkResponse> _articleLinkResponses, RedisCacheService _redis)
        {
            var result = new List<PageResult>();

            if (!_articleLinkResponses.Any())
            {
                Logger.LogWarning("No responses received, skipping fetching for {Name}", Name);
                return result;
            }

            foreach (var articleLinkResponse in _articleLinkResponses)
            {
                var rssResponseUri = articleLinkResponse.Uri;

                if (!ShouldFetchArticle(articleLinkResponse))
                {
                    Logger.LogInformation("{Url} for {Source} did not pass filter, skipping", rssResponseUri, Name);
                    continue;
                }

                Logger.LogInformation("Fetching: {URL}", rssResponseUri);

                var document = await FetchPage(rssResponseUri);
                ZonedDateTime publishedAt;
                var title = articleLinkResponse.Title;

                if (articleLinkResponse.PublishedAt != default)
                {
                    publishedAt = articleLinkResponse.PublishedAt;
                }
                else
                {
                    var publishedAtNodes = GetNodes(document, PublishedAtXPaths);
                    if (!ExtractPublishedAt(publishedAtNodes, rssResponseUri, out publishedAt))
                        continue;
                }

                var bodyNodes = GetNodes(document, BodyXPaths);
                if (!ExtractBody(bodyNodes, rssResponseUri, out var body))
                    continue;

                var imageNodes = GetNodes(document, ImageXPaths);
                if (!ExtractImage(imageNodes, rssResponseUri, out var imageUrl))
                    continue;

                var imageResult = await RestRequestHandler.SendGetRequestAsync(imageUrl!, fileDownloadParser, Logger);

                if (!imageResult.Success)
                {
                    Logger.LogWarning("No image could be downloaded for article: {URL}", rssResponseUri);
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
                    Url = rssResponseUri
                };

                var keyTitle = Regex.Replace(title!.Trim(), @"\s+", "_");
                keyTitle = Regex.Replace(keyTitle, @"[^\w\*]", "");

                var key = $"news_article:{Name}:{keyTitle}";
                if (await _redis.AddValue(key, newsArticle))
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

            return result;
        }

        protected async Task<HtmlDocument?> FetchPage(string _url)
        {
            try
            {
                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
                await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions {Headless = true});

                var page = await browser.NewPageAsync();
                var response = await page.GoToAsync(_url, WaitUntilNavigation.Networkidle0);

                if (response.Status == HttpStatusCode.OK)
                {
                    await ActRootOnPage(page);
                    var pageContent = await page.GetContentAsync();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(pageContent);

                    return doc;
                }

                Logger.LogWarning("Failed to send request to: {URL}. Response code back was: {StatusCode}", _url, response.Status);
                return null;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Exception when requesting page from {URL}", _url);
            }

            return null;
        }

        protected static HtmlNodeCollection? GetNodes(HtmlDocument? _page, string[] _xPaths)
        {
            return _xPaths
                .Select(_xPath => _page?.DocumentNode.SelectNodes(_xPath))
                .FirstOrDefault(_tag => _tag != null);
        }

        protected virtual bool ExtractTitle(HtmlNodeCollection? _node, string _url, out string? _value)
        {
            if (string.IsNullOrEmpty(_node?.First().InnerText))
            {
                Logger.LogWarning($"Title was empty for article: {{URL}}", _url);
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
                Logger.LogWarning($"Published at was empty for article: {{URL}}", _url);
                _value = default;
                return false;
            }

            _value = PublishedAtPattern
                .Parse(_node.First().InnerText)
                .Value.InUtc();

            return true;
        }

        protected virtual bool ExtractBody(HtmlNodeCollection? _node, string _url, out string? _value)
        {
            if (string.IsNullOrEmpty(_node?.First().InnerText))
            {
                Logger.LogWarning($"Body was empty for article: {{URL}}", _url);
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
                Logger.LogWarning($"Image src tag couldn't be found for article: {{URL}}", _url);
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

        protected virtual Task ActRootOnPage(Page _page)
        {
            return Task.CompletedTask;
        }
    }
}