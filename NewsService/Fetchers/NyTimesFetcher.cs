using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NewsService.Fetchers
{
    public class NyTimesFetcher : AbstractRssFetcher<NyTimesFetcher>
    {
        private const string NAME = "ny_times";

        public NyTimesFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }
    }
}