using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NewsService.Fetchers
{
    public class RecodeFetcher : AbstractWebPageFetcher<RecodeFetcher>
    {
        private const string NAME = "recode";

        public RecodeFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }
    }
}