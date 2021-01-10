using System;
using System.Threading.Tasks;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;

namespace NewsService.Fetchers.page
{
    public abstract class AbstractPageFetcher<T> : IPageFetcher, IDisposable
    {
        protected readonly WaitUntilNavigation WaitUntilNavigation;
        protected readonly ILogger Logger;
        protected Browser Browser;

        protected AbstractPageFetcher(ILoggerFactory _loggerFactory, WaitUntilNavigation _waitUntilNavigation)
        {
            WaitUntilNavigation = _waitUntilNavigation;
            Logger = _loggerFactory.CreateLogger<T>();
        }

        protected async Task Init()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            Browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        }

        public abstract Task<HtmlDocument?> FetchPage(string _url);
        public abstract Task<HtmlDocument?> FetchRootPage(string _url);

        protected virtual void Dispose(bool _disposing)
        {
            if (_disposing)
            {
                Browser.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}