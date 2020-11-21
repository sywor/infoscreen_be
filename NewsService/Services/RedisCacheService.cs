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
                return NewsResponse.FAILED("No article with key: " + _key);
            }

            var newsArticleResponse = await redis.Db0.GetAsync<NewsArticle>(_key);
            logger.LogInformation("Returned news article from Redis with key: {Key}", _key);
            return NewsResponse.SUCCESS(newsArticleResponse);
        }

        public async Task<List<NewsResponse>> GetValues(IEnumerable<string> _keys)
        {
            var keys = _keys.ToList();
            var newsArticleResponse = await redis.Db0.GetAllAsync<NewsArticle>(keys);
            logger.LogInformation("Returned news article from Redis with keys: {KeyCount}", keys.Count());

            return newsArticleResponse.Values.Select(NewsResponse.SUCCESS).ToList();
        }

        public Task<bool> AddValue(string _key, NewsArticle _value)
        {
            logger.LogInformation("Added news article to Redis with key: {Key}", _key);

            return redis.Db0.AddAsync(_key, _value);
        }

        public async Task<List<string>> GetKeys()
        {
            var keys = await redis.Db0.SearchKeysAsync("*");
            logger.LogInformation("Returned news article from Redis with keys: {Key}", keys);

            return keys.ToList();
        }
    }
}