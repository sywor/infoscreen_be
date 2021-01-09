using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Services;

namespace NewsService.Fetchers
{
    public class WiredFetcher : AbstractRssFetcher<WiredFetcher>
    {
        public const string NAME = "wired";

        public WiredFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, RedisCacheService _redis, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _redis, _loggerFactory)
        {
        }
    }
}