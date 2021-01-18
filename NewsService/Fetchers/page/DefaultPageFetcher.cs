using System;
using System.Net;
using System.Threading.Tasks;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;

namespace NewsService.Fetchers.page
{
    public class DefaultPageFetcher : AbstractPageFetcher<DefaultPageFetcher>
    {
        private DefaultPageFetcher(ILoggerFactory _loggerFactory, WaitUntilNavigation _waitUntilNavigation = WaitUntilNavigation.Networkidle0) : base(_loggerFactory, _waitUntilNavigation)
        {
        }

        public static async Task<IPageFetcher> Create(ILoggerFactory _loggerFactory, WaitUntilNavigation _waitUntilNavigation = WaitUntilNavigation.Networkidle0)
        {
            var instance = new DefaultPageFetcher(_loggerFactory, _waitUntilNavigation);
            await instance.Init();

            return instance;
        }

        public override async Task<HtmlDocument?> FetchRenderedPage(string _url)
        {
            try
            {
                var browserContext = await Browser.CreateIncognitoBrowserContextAsync();
                await using var page = await browserContext.NewPageAsync();
                var response = await page.GoToAsync(_url, WaitUntilNavigation);

                if (response.Status == HttpStatusCode.OK)
                {
                    var pageContent = await page.GetContentAsync();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(pageContent);

                    await page.CloseAsync();
                    await browserContext.CloseAsync();

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
            return FetchRenderedPage(_url);
        }
    }
}