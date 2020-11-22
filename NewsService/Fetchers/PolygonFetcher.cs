using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NewsService.Fetchers
{
    public class PolygonFetcher : AbstractRssFetcher<PolygonFetcher>
    {
        private const string NAME = "polygon";

        public PolygonFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }
    }
}