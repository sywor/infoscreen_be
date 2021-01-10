using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Fetchers.page;
using NewsService.Services;

namespace NewsService.Fetchers
{
    public class MashableFetcher : AbstractRssFetcher<MashableFetcher>
    {
        public const string NAME = "mashable";

        public MashableFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, RedisCacheService _redis, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _redis, _loggerFactory)
        {
            PageFetcher = DefaultPageFetcher.Create(_loggerFactory).Result;
        }
    }
}