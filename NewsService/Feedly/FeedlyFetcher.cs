using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Common.Recurrence;
using Common.Redis;

using Microsoft.Extensions.Logging;

using NewsService.Data;

using NodaTime;

namespace NewsService.Feedly
{
    public class FeedlyFetcher : IRunnable
    {
        private readonly FeedlyHttpClient feedlyClient;
        private readonly RedisCacheService redis;
        private readonly ILogger<FeedlyFetcher> logger;

        public FeedlyFetcher(ILoggerFactory _loggerFactory, FeedlyHttpClient _feedlyClient, RedisCacheService _redis)
        {
            logger = _loggerFactory.CreateLogger<FeedlyFetcher>();
            feedlyClient = _feedlyClient;
            redis = _redis;
        }

        public async Task Run()
        {
            foreach (var article in await feedlyClient.GetArticles())
            {
                var key = CreateRedisKey(article.Source, article.Fingerprint, article.Title);

                if (await redis.KeyExist(key))
                {
                    logger.LogInformation("Article with key: {Key} already exists, skipping", key);
                    continue;
                }
                await redis.AddValue(key, article);
                logger.LogInformation("Add news article: {Title}", article.Title);
            }
        }

        private static string CreateRedisKey(string _source, string _fingerprint, string _title)
        {
            static string Trim(string _str)
            {
                var str = _str.Trim();
                str = str.Replace("_", "");
                str = Regex.Replace(str, @"\s+", "_");
                str = Regex.Replace(str, @"[^\w\*]", "");
                return str.ToLower();
            }

            return $"news_article:{Trim(_source)}:{_fingerprint}:{Trim(_title)}";
        }

        private async Task StashFailedArticle(string _reason, Instant _unixTime, string _key)
        {
            var failedArticle = new FailedArticle
            {
                Reason = _reason,
                FetchedAt = _unixTime
            };

            await redis.AddValue($"failed_{_key}", failedArticle);
        }

    }
}