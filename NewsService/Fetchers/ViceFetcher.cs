using Microsoft.Extensions.Logging;

using NewsService.Config;

namespace NewsService.Fetchers
{
    public class ViceFetcher : AbstractWebPageFetcher<ViceFetcher>
    {
        private const string NAME = "vice";

        public ViceFetcher(NewsSourceConfigurations _newsSourceConfigurations, MinioConfiguration _minioConfiguration, ILoggerFactory _loggerFactory) :
            base(_newsSourceConfigurations, _minioConfiguration, NAME, _loggerFactory)
        {
        }
    }
}