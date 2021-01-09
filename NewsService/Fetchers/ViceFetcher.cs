using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Services;

namespace NewsService.Fetchers
{
    public class ViceFetcher : AbstractWebPageFetcher<ViceFetcher>
    {
        public const string NAME = "vice";

        public ViceFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, RedisCacheService _redis, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _redis, _loggerFactory)
        {
        }
    }
}