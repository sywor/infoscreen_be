using System;
using System.Threading.Tasks;

using Common.Bootstrap;
using Common.Config;

using Microsoft.Extensions.Logging;

using WeatherService.Services;

namespace WeatherService.Smhi
{
    public class SmhiFetcher : IRunnable
    {
        public SmhiFetcher(ILoggerFactory _loggerFactory, MinioConfiguration _minioConfiguration, IRedisCacheService _redis)
        {
            
        }

        public Task Run()
        {
            throw new NotImplementedException();
        }
    }
}