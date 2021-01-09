using Microsoft.Extensions.Logging;

using NewsService.Config;

namespace NewsService.Fetchers
{
    public class ReutersFetcher : AbstractWebPageFetcher<ReutersFetcher>
    {
        private const string NAME = "reuters";

        public ReutersFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _loggerFactory)
        {
        }
    }
}