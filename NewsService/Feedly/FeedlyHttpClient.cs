using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Common;
using Common.Config;
using Common.File;
using Common.Minio;
using Common.Redis;
using Common.Response;

using Microsoft.Extensions.Logging;

using NewsService.Config;
using NewsService.Data;

using NodaTime;

namespace NewsService.Feedly
{
    public class FeedlyHttpClient
    {
        private const string NoUrl = "No article url found for Feedly entry";
        private const string NoImage = "No image could be downloaded";
        private const string ContentLenghtToShort = "Content lenght to short";
        private const string NoAccessTokenRedis = "No access or refresh tokens found in redis";
        private const string NoAccessTokenGiven = "No access token returned from Feedly";
        private const string AuthUrl = "https://cloud.feedly.com/v3/auth/token";
        private const string FetchUrl = "https://cloud.feedly.com/v3/streams/contents?streamid=[STREAM_ID]&count=[COUNT]&ranked=[RANKED]";

        private readonly FeedlyConfiguration configuration;
        private readonly RedisCacheService redis;
        private readonly FileDownloadParser fileDownloadParser;
        private readonly ILogger<FeedlyHttpClient> logger;
        private readonly FeedlyAuthResponseParser authResponseParser;
        private readonly ArticleStreamResponseParser articleStreamResponseParser;
        private readonly List<NewsArticle> emptyResponse = new();

        public FeedlyHttpClient(ILoggerFactory _loggerFactory, FeedlyConfiguration _configuration, MinioConfiguration _minioConfiguration, RedisCacheService _redis)
        {
            logger = _loggerFactory.CreateLogger<FeedlyHttpClient>();
            configuration = _configuration;
            redis = _redis;
            fileDownloadParser = new FileDownloadParser(_minioConfiguration, _loggerFactory);
            authResponseParser = new FeedlyAuthResponseParser(_loggerFactory);
            articleStreamResponseParser = new ArticleStreamResponseParser(_loggerFactory);
        }

        public async Task<List<NewsArticle>> GetArticles()
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var cutoff = now.Minus(Duration.FromDays(1));

            var (accessTokenSuccess, tokens) = await GetAccessToken();

            if (!accessTokenSuccess)
            {
                return emptyResponse;
            }

            var (articleStreamSuccess, items) = await GetArticleSteamResult(tokens, cutoff);
            if (!articleStreamSuccess)
                return emptyResponse;

            items = items.Where(_entry => Instant.FromUnixTimeMilliseconds(_entry.Published) > cutoff).ToList();

            var result = await ParseNewsArticles(items, now);

            logger.LogInformation("Got {ArticleCount} articles", result.Count);

            return result;
        }

        private async Task<(bool Success, List<JItem> items)> GetArticleSteamResult(FeedlyTokens _tokens, Instant _cutoff)
        {
            var headers = new Dictionary<string, string>()
            {
                {"Authorization", $"Bearer {_tokens!.AccessToken}"}
            };

            var url = FetchUrl
                      .Replace("[STREAM_ID]", configuration.StreamId)
                      .Replace("[COUNT]", configuration.Count.ToString())
                      .Replace("[RANKED]", configuration.Ranked);

            var articleResponse = await RestRequestHandler.SendGetRequestAsync(url, headers, articleStreamResponseParser, logger);

            if (!articleResponse.Success)
            {
                return (false, new List<JItem>());
            }

            var articleStreamObj = ((FeedlyArticleStreamResponse)articleResponse).ArticleStream;

            var continuation = articleStreamObj.Continuation;
            var items = new List<JItem>();

            while (!string.IsNullOrEmpty(continuation))
            {
                var nextUrl = $"{url}&continuation={continuation}";
                items.AddRange(articleStreamObj.Items!);

                var oldest = items.Min(_entry => Instant.FromUnixTimeMilliseconds(_entry.Published));

                if (oldest < _cutoff)
                    break;

                articleResponse = await RestRequestHandler.SendGetRequestAsync(nextUrl, headers, articleStreamResponseParser, logger);
                articleStreamObj = ((FeedlyArticleStreamResponse)articleResponse).ArticleStream;
                continuation = articleStreamObj.Continuation;
            }

            return (true, items);
        }

        private async Task<List<NewsArticle>> ParseNewsArticles(IEnumerable<JItem> _items, Instant _now)
        {
            var result = new List<NewsArticle>();

            foreach (var item in _items.Where(Validate))
            {
                var (urlSuccess, articleUrl) = GetArticleUrl(item);
                if (!urlSuccess)
                    continue;

                var (imageSuccess, fileLocation) = await GetImage(item, articleUrl);
                if (!imageSuccess)
                    continue;

                var (contentSuccess, content) = GetContent(item, articleUrl);
                if (!contentSuccess)
                    continue;

                result.Add(new NewsArticle
                {
                    Content = content,
                    Fingerprint = item.Fingerprint!,
                    Source = item.Origin!.Title!,
                    Title = item.Title,
                    ArticleUrl = articleUrl,
                    FetchedAt = _now,
                    FileLocation = fileLocation,
                    OriginId = item.OriginId!,
                    PublishedAt = Instant.FromUnixTimeMilliseconds(item.Published)
                });
            }
            return result;
        }

        private (bool Success, string ArticleUrl) GetArticleUrl(JItem _item)
        {
            string articleUrl;
            if (_item.OriginId != null && Regex.IsMatch(_item.OriginId, @"https:\/\/|http:\/\/.+"))
            {
                articleUrl = _item.OriginId;
            }
            else if (_item.CanonicalUrl != null)
            {
                articleUrl = _item.CanonicalUrl;
            }
            else if (_item.Canonical != null && _item.Canonical.Any())
            {
                articleUrl = _item.Canonical.First().Href!;
            }
            else
            {
                logger.LogWarning(NoUrl);
                return (false, string.Empty);
            }
            return (true, articleUrl);
        }

        private async Task<(bool Success, MinioFile FileLocation)> GetImage(JItem _item, string _articleUrl)
        {
            IResponse imageResult;
            if (_item.Visual?.Url != null)
            {
                imageResult = await RestRequestHandler.SendGetRequestAsync(_item.Visual.Url, fileDownloadParser, logger);
            }
            else
            {
                logger.LogWarning("No image could be downloaded for article: {Url}", _articleUrl);
                return (false, default);
            }

            if (!imageResult.Success)
            {
                logger.LogWarning("No image could be downloaded for article: {Url}", _articleUrl);
                return (false, default);
            }

            return (true, ((FileDownloadResponse)imageResult).FileLocation);
        }

        private (bool Success, string Content) GetContent(JItem _item, string _articleUrl)
        {
            if (_item.Summary?.Content != null)
            {
                var content = Regex.Replace(_item.Summary.Content, "<.*?>", string.Empty);
                if (content.Length >= 100)
                    return (true, content);
            }

            logger.LogWarning("No content could be found for article: {Url}", _articleUrl);
            return (false, string.Empty);
        }

        private async Task<(bool Success, FeedlyTokens Tokens)> GetAccessToken()
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            var redisFeedlyResponse = await redis.GetValue<FeedlyTokens>("feedly_configuration");

            if (!redisFeedlyResponse.Success)
            {
                logger.LogError(NoAccessTokenRedis);
                return (false, new FeedlyTokens());
            }

            var tokens = ((RedisResponse<FeedlyTokens>)redisFeedlyResponse).Value;

            if ((tokens.Expires - Duration.FromDays(10)).ToInstant() >= now)
                return (true, tokens);

            logger.LogInformation("Access token expired, refreshing");

            var body = new List<KeyValuePair<string?, string?>>
            {
                new("refresh_token", tokens.RefreshToken),
                new("client_id", "feedlydev"),
                new("client_secret", "feedlydev"),
                new("grant_type", "refresh_token")
            };

            var authResponse = await RestRequestHandler.SendPostRequestAsync(AuthUrl, body, authResponseParser, logger);

            if (!authResponse.Success)
            {
                logger.LogError(NoAccessTokenGiven);
                return (false, new FeedlyTokens());
            }

            var authTokens = (FeedlyAuthResponse)authResponse;

            tokens.AccessToken = authTokens.AccessToken;
            tokens.Expires = (now + Duration.FromSeconds(authTokens.ExpiresIn)).InUtc();

            await redis.AddValue("feedly_configuration", tokens, TimeSpan.MaxValue);

            return (true, tokens);
        }

        private bool Validate(JItem _item)
        {
            if (string.IsNullOrEmpty(_item.Title))
            {
                logger.LogWarning("Failed validation for feedly entry: no title");
                return false;
            }
            if (_item.Summary == null)
            {
                logger.LogWarning("Failed validation for feedly entry: no summary node");
                return false;
            }
            if (_item.Origin == null)
            {
                logger.LogWarning("Failed validation for feedly entry: no origin node");
                return false;
            }
            if (_item.Visual == null)
            {
                logger.LogWarning("Failed validation for feedly entry: no visual node");
                return false;
            }
            if (_item.Published == default)
            {
                logger.LogWarning("Failed validation for feedly entry: no published");
                return false;
            }
            if (string.IsNullOrEmpty(_item.Summary.Content))
            {
                logger.LogWarning("Failed validation for feedly entry: no content");
                return false;
            }
            if (string.IsNullOrEmpty(_item.Origin.Title))
            {
                logger.LogWarning("Failed validation for feedly entry: no source name");
                return false;
            }
            if (string.IsNullOrEmpty(_item.Visual.Url) ||
                !Regex.IsMatch(_item.Visual.Url, @"https:\/\/|http:\/\/.+"))
            {
                logger.LogWarning("Failed validation for feedly entry: no image url");
                return false;
            }

            return true;
        }
    }
}