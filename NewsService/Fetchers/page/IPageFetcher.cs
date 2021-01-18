using System;
using System.Threading.Tasks;

using HtmlAgilityPack;

namespace NewsService.Fetchers.page
{
    public interface IPageFetcher : IDisposable
    {
        Task<HtmlDocument?> FetchRenderedPage(string _url);
        Task<HtmlDocument?> FetchRootPage(string _url);
        Task<HtmlDocument?> FetchRawPage(string _url);
    }
}