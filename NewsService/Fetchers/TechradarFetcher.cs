using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Services;

namespace NewsService.Fetchers
{
    public class TechradarFetcher : AbstractRssFetcher<TechradarFetcher>
    {
        public const string NAME = "techradar";

        public TechradarFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, RedisCacheService _redis, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _redis, _loggerFactory)
        {
        }
    }
}