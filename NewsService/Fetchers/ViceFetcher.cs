using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NewsService.Fetchers
{
    public class ViceFetcher : AbstractWebPageFetcher<ViceFetcher>
    {
        private const string NAME = "vice";

        public ViceFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }
    }
}