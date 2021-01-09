using System;
using System.Net;
using System.Threading.Tasks;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;

using Serilog;

namespace NewsService.Fetchers.page
{
    public class DefaultPageFetcher : AbstractPageFetcher<DefaultPageFetcher>
    {
        public DefaultPageFetcher(ILoggerFactory _loggerFactory) : base(_loggerFactory)
        {
        }

        public override async Task<HtmlDocument?> FetchPage(string _url)
        {
            try
            {
                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
                await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
                var context = await browser.CreateIncognitoBrowserContextAsync();

                var page = await context.NewPageAsync();

                var response = await page.GoToAsync(_url, WaitUntilNavigation.Networkidle2);

                if (response.Status == HttpStatusCode.OK)
                {
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

        public override Task<HtmlDocument?> FetchRootPage(string _url)
        {
            return FetchPage(_url);
        }
    }
}