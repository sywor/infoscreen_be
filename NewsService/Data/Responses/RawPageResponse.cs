using HtmlAgilityPack;

using NewsService.Data.Parsers;

namespace NewsService.Data.Responses
{
    public class RawPageResponse : IResponse
    {
        public bool Success => true;

        public HtmlDocument HtmlDocument { get; set; }
    }
}