using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NewsService.Data;

namespace NewsService.Services
{
    public class NewsHandlerService
    {
        private readonly ILogger logger;
        private readonly RedisCacheService redis;

        public NewsHandlerService(ILoggerFactory _loggerFactory, RedisCacheService _redis)
        {
            logger = _loggerFactory.CreateLogger<NewsHandlerService>();
            redis = _redis;
        }

        public async Task<IEnumerable<NewsResponse>> NewsArticles()
        {
            var keys = await redis.GetKeys();

            if (!keys.Any())
            {
                return SingleEnumerable<NewsResponse>.Of(NewsResponse.Failed("No keys found"));
            }
            
            return await redis.GetValues(keys);
        }

        public Task<NewsResponse> GetArticle(string _articleKey)
        {
            return redis.GetValue(_articleKey);
        }
    }
}