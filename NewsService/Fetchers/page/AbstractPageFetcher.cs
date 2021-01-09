using System.Threading.Tasks;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using PuppeteerSharp;

using Serilog;

using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace NewsService.Fetchers.page
{
    public abstract class AbstractPageFetcher<T> : IPageFetcher
    {
        protected readonly ILogger Logger;

        protected AbstractPageFetcher(ILoggerFactory _loggerFactory)
        {
            Logger = _loggerFactory.CreateLogger<T>();
        }

        public abstract Task<HtmlDocument?> FetchPage(string _url);
        public abstract Task<HtmlDocument?> FetchRootPage(string _url);
    }
}