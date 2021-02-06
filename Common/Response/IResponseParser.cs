using System.Net.Http;
using System.Threading.Tasks;

namespace Common.Response
{
    public interface IResponseParser
    {
        Task<IResponse> ParseAsync(HttpContent _responseContent);
    }
}