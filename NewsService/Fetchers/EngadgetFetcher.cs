using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NewsService.Fetchers
{
    public class EngadgetFetcher : AbstractWebPageFetcher<EngadgetFetcher>
    {
        private const string NAME = "engadget";

        public EngadgetFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }
    }
}