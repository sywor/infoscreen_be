using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Common.Redis;

using Microsoft.Extensions.Logging;

using NewsService.Data;
using NewsService.Feedly;

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
            var keys = await redis.GetKeys("news_article:*");

            if (keys.Any())
            {
                return (await redis.GetValues<NewsArticle>(keys))
                       .Where(_response => _response.Success)
                       .Select(_response =>
                       {
                           var redisResponse = (RedisResponse<NewsArticle>) _response;

                           return new NewsResponse
                           {
                               Key = redisResponse.Key,
                               NewsArticle = redisResponse.Value
                           };
                       })
                       .ToList();
            }

            logger.LogWarning("No keys found for news articles");
            return new List<NewsResponse>();
        }

        public async Task<NewsResponse?> GetArticle(string _articleKey)
        {
            var article = await redis.GetValue<NewsArticle>(_articleKey);

            if (!article.Success)
            {
                logger.LogWarning("No keys found for news article with key {Key}", _articleKey);
                return null;
            }

            var redisResponse = (RedisResponse<NewsArticle>) article;

            return new NewsResponse()
            {
                Key = redisResponse.Key,
                NewsArticle = redisResponse.Value
            };
        }
    }
}