using System;
using System.Net;
using System.Threading.Tasks;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;

namespace NewsService.Fetchers.page
{
    public class MashablePageFetcher : AbstractPageFetcher<MashablePageFetcher>
    {
        private MashablePageFetcher(ILoggerFactory _loggerFactory, WaitUntilNavigation _waitUntilNavigation = WaitUntilNavigation.Networkidle0) : base(_loggerFactory, _waitUntilNavigation)
        {
        }

        public static async Task<IPageFetcher> Create(ILoggerFactory _loggerFactory, WaitUntilNavigation _waitUntilNavigation = WaitUntilNavigation.Networkidle0)
        {
            var instance = new MashablePageFetcher(_loggerFactory, _waitUntilNavigation);
            await instance.Init();

            return instance;
        }

        public override async Task<HtmlDocument?> FetchPage(string _url)
        {
            try
            {
                var browserContext = await Browser.CreateIncognitoBrowserContextAsync();
                await using var page = await browserContext.NewPageAsync();
                var response = await page.GoToAsync(_url, WaitUntilNavigation);

                if (response.Status == HttpStatusCode.OK)
                {
                    await page.ClickAsync("button#_evidon-banner-acceptbutton.evidon-barrier-acceptbutton");
                    await page.EvaluateExpressionAsync("window.scrollBy(0, document.body.scrollHeight * 6)");
                    await page.WaitForTimeoutAsync(2000);

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