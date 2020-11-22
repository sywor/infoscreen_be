using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NewsService.Fetchers
{
    public class CnbcFetcher : AbstractRssFetcher<CnbcFetcher>
    {
        private const string NAME = "cnbc";

        public CnbcFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }
    }
}