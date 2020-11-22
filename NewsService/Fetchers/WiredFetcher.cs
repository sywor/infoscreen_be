using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NewsService.Fetchers
{
    public class WiredFetcher : AbstractRssFetcher<WiredFetcher>
    {
        private const string NAME = "wired";

        public WiredFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }
    }
}