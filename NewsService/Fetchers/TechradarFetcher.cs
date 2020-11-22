using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NewsService.Fetchers
{
    public class TechradarFetcher : AbstractRssFetcher<TechradarFetcher>
    {
        private const string NAME = "techradar";

        public TechradarFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }
    }
}