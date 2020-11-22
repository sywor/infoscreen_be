using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NewsService.Fetchers
{
    public class TheVergeFetcher : AbstractRssFetcher<TheVergeFetcher>
    {
        private const string NAME = "the_verge";

        public TheVergeFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }
    }
}