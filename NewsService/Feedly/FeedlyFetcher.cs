using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using FeedlySharp;
using FeedlySharp.Models;

using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Data.Parsers;
using NewsService.Fetchers;
using NewsService.Services;

using NodaTime;
using NodaTime.Extensions;

namespace NewsService.Feedly
{
    public class FeedlyFetcher
    {
        private readonly FeedlyConfiguration feedlyConfiguration;
        private readonly RedisCacheService redis;
        private readonly ILogger<FeedlyFetcher> logger;
        private readonly FileDownloadParser fileDownloadParser;
        private static readonly Random Random = new Random();

        public FeedlyFetcher(ILoggerFactory _loggerFactory, FeedlyConfiguration _feedlyConfiguration, MinioConfiguration _minioConfiguration, RedisCacheService _redis)
        {
            logger = _loggerFactory.CreateLogger<FeedlyFetcher>();
            feedlyConfiguration = _feedlyConfiguration;
            redis = _redis;
            fileDownloadParser = new FileDownloadParser(_minioConfiguration, _loggerFactory);
        }

        public async Task Fetch()
        {
            var options = new FeedlyOptions()
            {
                AccessToken = feedlyConfiguration.AccessToken,
                RefreshToken = feedlyConfiguration.RefreshToken,
                UserID = feedlyConfiguration.UserID
            };


            var feedlySharp = new FeedlySharpHttpClient(options);

            var now = SystemClock.Instance.GetCurrentInstant();
            var cutoff = now.Minus(Duration.FromDays(1));

            var streamOptions = new StreamOptions()
            {
                Count = 500,
                StreamId = "user/6bbcd123-ab40-4b41-b2d8-0f4e105e2069/category/e1de0087-91c9-4014-a259-c3463fa16ab1",
                Ranked = RankType.Newest
            };

            var response = await feedlySharp.GetStream(streamOptions);
            var entries = new List<Entry>();

            while (!string.IsNullOrEmpty(response.Continuation))
            {
                streamOptions.Continuation = response.Continuation;
                entries.AddRange(response.Items);

                var oldest = ZonedDateTime.FromDateTimeOffset(response.Items.Min(_entry => _entry.Published));

                if (oldest.ToInstant() < cutoff)
                    break;

                response = await feedlySharp.GetStream(streamOptions);
            }

            entries = entries.Where(_entry => ZonedDateTime.FromDateTimeOffset(_entry.Published).ToInstant() > cutoff).ToList();

            logger.LogInformation("Got {ArticleCount} to fetch", entries.Count);

            var counter = 0;

            foreach (var entry in entries.Where(Validate))
            {
                string articleUrl;
                if (Regex.IsMatch(entry.OriginId, @"https:\/\/|http:\/\/.+"))
                {
                    articleUrl = entry.OriginId;
                }
                else if (entry.Canonical.Any())
                {
                    articleUrl = entry.Canonical.First().Href;
                }
                else
                {
                    logger.LogWarning("No article url found for Feedly entry");
                    continue;
                }

                var imageResult = await RestRequestHandler.SendGetRequestAsync(entry.Visual.Url, fileDownloadParser, logger);

                if (!imageResult.Success)
                {
                    logger.LogWarning("No image could be downloaded for article: {Url}", articleUrl);

                    continue;
                }
                var imageUrl = ((FileDownloadResponse) imageResult).FileUri;
                var publishedAt = ZonedDateTime.FromDateTimeOffset(entry.Published);
                var content = Regex.Replace(entry.Summary.Content, "<.*?>", string.Empty);
                var fetchedAt = now.ToDateTimeOffset().ToZonedDateTime();

                if (content.Length < 100)
                {
                    logger.LogWarning("Content lenght to short");
                    continue;
                }

                var newsArticle = new NewsArticle
                {
                    OriginId = entry.OriginId,
                    Fingerprint = entry.Fingerprint,
                    Title = entry.Title,
                    Source = entry.Origin.Title,
                    PublishedAt = publishedAt,
                    Content = content,
                    ImageUrl = imageUrl,
                    FetchedAt = fetchedAt,
                    ArticleUrl = articleUrl
                };

                var key = CreateRedisKey(newsArticle);
                await redis.AddValue(key, newsArticle);

                logger.LogInformation("Add news article: {Title}", newsArticle.Title);
                counter++;
            }

            logger.LogInformation("Done! Got {ArticleCount} articles", counter);
        }

        private bool Validate(Entry _entry)
        {
            if (string.IsNullOrEmpty(_entry.Title))
            {
                logger.LogWarning("Failed validation for feedly entry: no title");
                return false;
            }
            if (_entry.Summary == null)
            {
                logger.LogWarning("Failed validation for feedly entry: no summary node");
                return false;
            }
            if (_entry.Origin == null)
            {
                logger.LogWarning("Failed validation for feedly entry: no origin node");
                return false;
            }
            if (_entry.Visual == null)
            {
                logger.LogWarning("Failed validation for feedly entry: no visual node");
                return false;
            }
            if (_entry.Published == default)
            {
                logger.LogWarning("Failed validation for feedly entry: no published");
                return false;
            }
            if (string.IsNullOrEmpty(_entry.Summary.Content))
            {
                logger.LogWarning("Failed validation for feedly entry: no content");
                return false;
            }
            if (string.IsNullOrEmpty(_entry.Origin.Title))
            {
                logger.LogWarning("Failed validation for feedly entry: no source name");
                return false;
            }
            if (string.IsNullOrEmpty(_entry.Visual.Url) ||
                !Regex.IsMatch(_entry.Visual.Url, @"https:\/\/|http:\/\/.+"))
            {
                logger.LogWarning("Failed validation for feedly entry: no image url");
                return false;
            }

            return true;
        }

        private static string CreateRedisKey(NewsArticle _newsArticle)
        {
            static string Trim(string _str)
            {
                var str = _str.Trim();
                str = str.Replace("_", "");
                str = Regex.Replace(str, @"\s+", "_");
                str = Regex.Replace(str, @"[^\w\*]", "");
                return str;
            }

            return $"news_article:{Trim(_newsArticle.Source)}:{_newsArticle.Fingerprint}:{Trim(_newsArticle.Title)}";
        }
    }
}