using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NewsService.Fetchers
{
    public class NationalGeographicFetcher : AbstractWebPageFetcher<NationalGeographicFetcher>
    {
        private const string NAME = "national_geographic";

        public NationalGeographicFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }
    }
}