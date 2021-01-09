using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Fetchers.page;
using NewsService.Services;

namespace NewsService.Fetchers
{
    public class NationalGeographicFetcher : AbstractWebPageFetcher<NationalGeographicFetcher>
    {
        public const string NAME = "national_geographic";

        public NationalGeographicFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, RedisCacheService _redis, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _redis, _loggerFactory, new DefaultPageFetcher(_loggerFactory))
        {
        }
    }
}