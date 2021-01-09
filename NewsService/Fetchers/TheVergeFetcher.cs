using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Services;

namespace NewsService.Fetchers
{
    public class TheVergeFetcher : AbstractRssFetcher<TheVergeFetcher>
    {
        public const string NAME = "the_verge";

        public TheVergeFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, RedisCacheService _redis, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _redis, _loggerFactory)
        {
        }
    }
}