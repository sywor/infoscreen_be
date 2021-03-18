using System.Net.Http;
using System.Threading.Tasks;

using Common.Response;

namespace WashingMachineService.WashingMachine
{
    public class RefreshTokenParser : IResponseParser
    {

        public Task<IResponse> ParseAsync(HttpContent _responseContent)
        {
            throw new System.NotImplementedException();
        }
    }
}