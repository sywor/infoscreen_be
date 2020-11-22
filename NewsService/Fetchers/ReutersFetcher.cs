using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NewsService.Fetchers
{
    public class ReutersFetcher : AbstractWebPageFetcher<ReutersFetcher>
    {
        private const string NAME = "reuters";

        public ReutersFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }
    }
}