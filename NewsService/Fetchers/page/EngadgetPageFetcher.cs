using System;
using System.Net;
using System.Threading.Tasks;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;

namespace NewsService.Fetchers.page
{
    public class EngadgetPageFetcher : AbstractPageFetcher<EngadgetFetcher>
    {
        private EngadgetPageFetcher(ILoggerFactory _loggerFactory) : base(_loggerFactory, WaitUntilNavigation.Networkidle0)
        {
        }

        public static async Task<IPageFetcher> Create(ILoggerFactory _loggerFactory)
        {
            var instance = new EngadgetPageFetcher(_loggerFactory);
            await instance.Init();

            return instance;
        }

        public override async Task<HtmlDocument?> FetchPage(string _url)
        {
            return await Fetch(_url, false);
        }

        public override async Task<HtmlDocument?> FetchRootPage(string _url)
        {
            return await Fetch(_url, true);
        }

        private async Task<HtmlDocument?> Fetch(string _url, bool _rootPage)
        {
            try
            {
                var browserContext = await Browser.CreateIncognitoBrowserContextAsync();
                var page = await browserContext.NewPageAsync();
                var response = await page.GoToAsync(_url, WaitUntilNavigation);

                if (response.Status == HttpStatusCode.OK)
                {
                    await page.ClickAsync("button.btn.primary");

                    if (_rootPage)
                        await page.WaitForXPathAsync("//div[contains(@id, 'Page')]//div[contains(@id, 'module-latest')]");
                    else
                        await page.WaitForXPathAsync("//nav[@id='engadget-global-nav']");

                    await page.WaitForTimeoutAsync(1000);

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
    }
}