using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NewsService.Fetchers
{
    public class MashableFetcher : AbstractRssFetcher<MashableFetcher>
    {
        private const string NAME = "mashable";

        public MashableFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }
    }
}