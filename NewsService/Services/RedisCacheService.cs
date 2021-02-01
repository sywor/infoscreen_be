using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NewsService.Data;
using NewsService.Feedly;

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

                return NewsResponse.Failed("No article with key: " + _key);
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

            var response = new List<NewsResponse>();
            
            foreach (var (key, value) in newsArticleResponse)
            {
                response.Add(new NewsResponse
                {
                    Key = key,
                    NewsArticle = value,
                    Success = true
                });
            }

            return response;
        }

        public Task<bool> AddValue(string _key, NewsArticle _value)
        {
            logger.LogInformation("Added news article to Redis with key: {Key}", _key);

            return redis.Db0.AddAsync(_key, _value, TimeSpan.FromDays(2));
        }

        public async Task<List<string>> GetKeys(string _searchPattern)
        {
            var keys = await redis.Db0.SearchKeysAsync(_searchPattern);
            var result = keys.ToList();
            logger.LogDebug("Returned news article keys from Redis {Count}", result.Count());

            return result;
        }

        public async Task<bool> KeyExist(string _key)
        {
            var keys = await GetKeys("news_article:*");

            return keys.Contains(_key);
        }
    }
}