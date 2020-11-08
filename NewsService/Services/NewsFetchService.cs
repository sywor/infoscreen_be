using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NewsService.Services
{
    public class NewsFetchService
    {
        private readonly ILogger logger;
        private readonly RedisCacheService redis;

        public NewsFetchService(ILoggerFactory _loggerFactory, RedisCacheService _redis)
        {
            logger = _loggerFactory.CreateLogger<NewsFetchService>();
            redis = _redis;
        }

        public async Task<List<string>> Fetch()
        {
            throw new NotImplementedException();
        }
    }
}