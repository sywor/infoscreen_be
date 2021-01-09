using Microsoft.Extensions.Logging;

using NewsService.Config;

namespace NewsService.Fetchers
{
    public class IgnFetcher : AbstractRssFetcher<IgnFetcher>
    {
        private const string NAME = "ign";

        public IgnFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _loggerFactory)
        {
        }
    }
}