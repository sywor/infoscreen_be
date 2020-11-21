using System.Net.Http;
using System.Threading.Tasks;

namespace NewsService.Data.Parsers
{
    public interface IResponseParser
    {
        Task<IResponse> ParseAsync(HttpContent _responseContent);
    }
}