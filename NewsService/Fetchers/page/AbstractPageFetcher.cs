using System;
using System.Threading.Tasks;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using NewsService.Data.Parsers;
using NewsService.Data.Responses;

using PuppeteerSharp;

namespace NewsService.Fetchers.page
{
    public abstract class AbstractPageFetcher<T> : IPageFetcher
    {
        private readonly RawPageParser rawPageParser;

        protected readonly WaitUntilNavigation WaitUntilNavigation;
        protected readonly ILogger Logger;
        protected Browser Browser;


        protected AbstractPageFetcher(ILoggerFactory _loggerFactory, WaitUntilNavigation _waitUntilNavigation)
        {
            WaitUntilNavigation = _waitUntilNavigation;
            Logger = _loggerFactory.CreateLogger<T>();
            rawPageParser = new RawPageParser();
        }

        protected async Task Init()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            Browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        }

        public abstract Task<HtmlDocument?> FetchRenderedPage(string _url);

        public virtual Task<HtmlDocument?> FetchRootPage(string _url)
        {
            return FetchRenderedPage(_url);
        }

        public virtual async Task<HtmlDocument?> FetchRawPage(string _url)
        {
            var sendGetRequestAsync = await RestRequestHandler.SendGetRequestAsync(_url, rawPageParser, Logger);

            return sendGetRequestAsync.Success ? ((RawPageResponse) sendGetRequestAsync).HtmlDocument : null;
        }

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