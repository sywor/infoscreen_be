using Microsoft.Extensions.Logging;

using NewsService.Config;

namespace NewsService.Fetchers
{
    public class TechradarFetcher : AbstractRssFetcher<TechradarFetcher>
    {
        private const string NAME = "techradar";

        public TechradarFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _loggerFactory)
        {
        }
    }
}