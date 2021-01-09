using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Services;

namespace NewsService.Fetchers
{
    public class NyTimesFetcher : AbstractRssFetcher<NyTimesFetcher>
    {
        public const string NAME = "ny_times";

        public NyTimesFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, RedisCacheService _redis, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _redis, _loggerFactory)
        {
        }
    }
}