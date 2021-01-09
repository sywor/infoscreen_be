using Microsoft.Extensions.Logging;

using NewsService.Config;

namespace NewsService.Fetchers
{
    public class NationalGeographicFetcher : AbstractWebPageFetcher<NationalGeographicFetcher>
    {
        private const string NAME = "national_geographic";

        public NationalGeographicFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _loggerFactory)
        {
        }
    }
}