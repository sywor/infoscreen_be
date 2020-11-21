using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
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
using SimpleFeedReader;

namespace NewsService.Fetchers
{
    public abstract class AbstractFetcher<T> : IFetcher
    {
        protected string Name { get; }

        protected enum PageType
        {
            RSS,
            WEB_PAGE
        }

        private readonly FileDownloadParser fileDownloadParser;
        private readonly MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
        protected readonly ILogger Logger;
        protected readonly string RootUrl;
        protected readonly string[] TitleXPaths;
        protected readonly string[] ImageXPaths;
        protected readonly string[] BodyXPaths;
        protected readonly string[] PublishedAtXPaths;
        protected readonly InstantPattern PublishedAtPattern;
        protected readonly PageType Type;

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

            if (configuration["Url"] == null)
            {
                Logger.LogError("Url cannot be null: {Page}", _name);
                throw new ArgumentException($"Url cannot be null: {_name}");
            }

            RootUrl = configuration["Url"];

            switch (configuration["Type"])
            {
                case "RSS":
                    Type = PageType.RSS;
                    break;
                case "WEB_PAGE":
                    Type = PageType.WEB_PAGE;
                    break;
                default:
                    Logger.LogError($"Invalid page type: {configuration["Type"]}");
                    throw new ArgumentException();
            }

            var publishedAtConfig = configuration["PublishedAtPattern"];

            if (publishedAtConfig == null)
            {
                Logger.LogError("Published at cannot be null: {Page}", _name);
                throw new ArgumentException($"Published at cannot be null: {_name}");
            }

            PublishedAtPattern = InstantPattern.Create(publishedAtConfig, CultureInfo.InvariantCulture);

            foreach (var xpath in configuration.GetSection("XPaths").GetChildren())
            {
                switch (xpath.Key)
                {
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

        public async Task<IEnumerable<PageResult>> Fetch(RedisCacheService _redis)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var fetchTime = now.InUtc();

            return Type switch
            {
                PageType.RSS => await FetchRss(RootUrl, fetchTime, _redis),
                PageType.WEB_PAGE => await FetchWebPage(RootUrl, fetchTime, _redis)
            };
        }

        private async Task<IEnumerable<PageResult>> FetchRss(string _rootUrl, ZonedDateTime _fetchTime, RedisCacheService _redis)
        {
            var reader = new FeedReader();
            var items = reader.RetrieveFeed(RootUrl);
            var urls = items.Select(_item => _item.Uri.AbsoluteUri).ToList();

            return await FetchAndParseArticle(_fetchTime, urls, _redis);
        }

        private async Task<IEnumerable<PageResult>> FetchWebPage(string _url, ZonedDateTime _fetchTime, RedisCacheService _redis)
        {
            var document = await FetchPage(_url);


            return null;
        }

        private async Task<List<PageResult>> FetchAndParseArticle(ZonedDateTime _fetchTime, IEnumerable<string> _urls, RedisCacheService _redis)
        {
            var result = new List<PageResult>();

            foreach (var url in _urls)
            {
                if (!ShouldFetchArticle(url))
                {
                    Logger.LogInformation("{Url} for {Source} did not pass filter, skipping", url, Name);
                    continue;
                }

                Logger.LogInformation("Fetching: {URL}", url);

                var document = await FetchPage(url);
                var titleNode = GetSingleNode(document, TitleXPaths);

                if (ValueInvalid(titleNode, url, "Title"))
                    continue;

                var publishedAtNode = GetSingleNode(document, PublishedAtXPaths);
                if (ValueInvalid(publishedAtNode, url, "Published at"))
                    continue;

                var bodyNode = GetSingleNode(document, BodyXPaths);
                if (ValueInvalid(bodyNode, url, "Body"))
                    continue;

                var imageNode = GetSingleNode(document, ImageXPaths);
                var imageUrl = GetImageUrl(imageNode);
                if (imageUrl == null)
                {
                    Logger.LogWarning("No image could be found for article: {URL}", url);
                    continue;
                }

                var imageResult = await RESTRequestHandler.SendGetRequestAsync(imageUrl, fileDownloadParser, Logger);

                if (!imageResult.Success)
                {
                    Logger.LogWarning("No image could be downloaded for article: {URL}", url);
                    continue;
                }

                var imagePath = ((FileDownloadResponse) imageResult).FileUri;

                var publishedAt = PublishedAtPattern
                    .Parse(publishedAtNode.InnerText)
                    .Value.InUtc();

                var newsArticle = new NewsArticle
                {
                    Title = titleNode.InnerText,
                    Source = Name,
                    PublishedAt = publishedAt,
                    Content = bodyNode.InnerText,
                    ImagePath = imagePath,
                    FetchedAt = _fetchTime,
                    Url = url
                };

                var hash = CalculateKey(newsArticle);
                if (await _redis.AddValue(hash, newsArticle))
                {
                    result.Add(new PageResult {ArticleKey = hash});
                }
            }

            return result;
        }

        private async Task<HtmlDocument> FetchPage(string _url)
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions {Headless = true});

            var page = await browser.NewPageAsync();
            var response = await page.GoToAsync(_url);

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

        private static HtmlNode? GetSingleNode(HtmlDocument _page, string[] _xPaths)
        {
            return _xPaths
                .Select(_xPath => _page.DocumentNode.SelectSingleNode(_xPath))
                .FirstOrDefault(_tag => _tag != null) ?? null;
        }

        private bool ValueInvalid(HtmlNode? _node, string _url, string _msg)
        {
            if (_node == null)
            {
                Logger.LogWarning($"{_msg} could be found for article: {{URL}}", _url);
                return true;
            }

            if (string.IsNullOrEmpty(_node.InnerText))
            {
                Logger.LogWarning($"{_msg} was empty for article: {{URL}}", _url);
                return true;
            }

            return false;
        }

        private string CalculateKey(NewsArticle _newsArticle)
        {
            var key = $"{Name}:{_newsArticle.Title}";
            var bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(key));
            var hash = new StringBuilder();

            foreach (var t in bytes)
            {
                hash.Append(t.ToString("x2"));
            }

            return hash.ToString();
        }

        protected virtual bool ShouldFetchArticle(string _url)
        {
            return true;
        }

        protected virtual Task ActRootOnPage(Page _page)
        {
            return Task.CompletedTask;
        }

        protected virtual string? GetImageUrl(HtmlNode? _imageTag)
        {
            return _imageTag?.GetAttributeValue("src", null);
        }

        protected virtual string SanitizeArticleText(string _text)
        {
            return _text;
        }

        protected virtual string? GetArticleText(HtmlDocument _page, string[] _xPaths)
        {
            return GetSingleNode(_page, _xPaths)?.InnerText;
        }
    }
}