using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NewsService.Data;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace NewsService.Services
{
    public class RedisCacheService
    {
        private readonly ILogger logger;
        private readonly IRedisCacheClient redis;

        public RedisCacheService(ILoggerFactory _loggerFactory, IRedisCacheClient _redis)
        {
            logger = _loggerFactory.CreateLogger<RedisCacheService>();
            redis = _redis;
        }

        public async Task<NewsResponse> GetValue(string _key)
        {
            if (!await redis.Db0.ExistsAsync(_key))
            {
                logger.LogWarning("Could not find news article in Redis with key: {Key}", _key);
                return NewsResponse.FAILED();
            }

            var newsArticleResponse = await redis.Db0.GetAsync<NewsArticle>(_key);
            logger.LogInformation("Returned news article from Redis with key: {Key}", _key);
            return NewsResponse.SUCCESS(newsArticleResponse);
        }

        public async IAsyncEnumerable<NewsResponse> GetValues(IEnumerable<string> _keys)
        {
            var newsArticleResponse = await redis.Db0.GetAllAsync<NewsArticle>(_keys);
            logger.LogInformation("Returned news article from Redis with keys: {Key}", _keys);

            foreach (var response in newsArticleResponse.Values.Select(NewsResponse.SUCCESS))
            {
                yield return response;
            }
        }

        public Task<bool> AddValue(string _key, NewsArticle _value)
        {
            logger.LogInformation("Added news article to Redis with key: {Key}", _key);
            return redis.Db0.AddAsync(_key, _value, TimeSpan.FromDays(1));
        }
    }
}