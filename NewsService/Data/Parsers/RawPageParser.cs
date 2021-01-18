using System.Net.Http;
using System.Threading.Tasks;

using HtmlAgilityPack;

using NewsService.Data.Responses;

namespace NewsService.Data.Parsers
{
    public class RawPageParser : IResponseParser
    {
        public async Task<IResponse> ParseAsync(HttpContent _responseContent)
        {
            var response = await _responseContent.ReadAsStringAsync();

            if (response == null)
                return FailureResponse.Instance;

            var doc = new HtmlDocument();
            doc.LoadHtml(response);

            return new RawPageResponse
            {
                HtmlDocument = doc
            };
        }
    }
}