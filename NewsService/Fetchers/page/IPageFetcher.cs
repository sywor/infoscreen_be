using System.Threading.Tasks;

using HtmlAgilityPack;

namespace NewsService.Fetchers.page
{
    public interface IPageFetcher
    {
        Task<HtmlDocument?> FetchPage(string _url);
        Task<HtmlDocument?> FetchRootPage(string _url);
    }
}