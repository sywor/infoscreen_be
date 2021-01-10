using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Fetchers.page;
using NewsService.Services;

namespace NewsService.Fetchers
{
    public class ReutersFetcher : AbstractWebPageFetcher<ReutersFetcher>
    {
        public const string NAME = "reuters";

        public ReutersFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, RedisCacheService _redis, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _redis, _loggerFactory)
        {
            PageFetcher = DefaultPageFetcher.Create(_loggerFactory).Result;
        }
    }
}