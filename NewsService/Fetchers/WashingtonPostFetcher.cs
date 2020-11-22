using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NewsService.Fetchers
{
    public class WashingtonPostFetcher : AbstractRssFetcher<WashingtonPostFetcher>
    {
        private const string NAME = "washington_post";

        public WashingtonPostFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }
    }
}