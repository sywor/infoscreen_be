using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NewsService.Data;

namespace NewsService.Services
{
    public class NewsHandlerService
    {
        public enum State
        {
            FETCHING,
            READY
        }

        private readonly ILogger logger;
        private readonly NewsFetchService fetchService;
        private readonly RedisCacheService redis;
        private readonly Timer timer;
        private List<string> articleKeys;
        private string currentArticleKey;

        public State Status { get; private set; } = State.FETCHING;

        public DateTime LastUpdate { get; private set; } = DateTime.MinValue;

        public NewsHandlerService(ILoggerFactory _loggerFactory, NewsFetchService _fetchService, RedisCacheService _redis)
        {
            logger = _loggerFactory.CreateLogger<NewsHandlerService>();
            fetchService = _fetchService;
            redis = _redis;

            timer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        }

        private async void TimerCallback(object? _state)
        {
            Status = State.FETCHING;
            var time = Stopwatch.StartNew();
            logger.LogInformation("Fetching news");


            articleKeys = await fetchService.Fetch();


            time.Stop();
            logger.LogInformation("Done fetching news. Took: {Took} and fetched {Count} articles", time.Elapsed, 0);
            LastUpdate = DateTime.UtcNow;
            Status = State.READY;
        }

        public IAsyncEnumerable<NewsResponse> NewsArticles()
        {
            return Status == State.FETCHING
                ? SingleAsyncEnumerable<NewsResponse>.Of(NewsResponse.FAILED("Fetcher service is currently fetching"))
                : redis.GetValues(articleKeys);
        }

        public Task<NewsResponse> GetArticle(string _articleKey)
        {
            return Status == State.FETCHING
                ? Task.FromResult(NewsResponse.FAILED("Fetcher is running"))
                : redis.GetValue(_articleKey);
        }

        public Task<NewsResponse> GetNextArticle()
        {
            return Status == State.FETCHING
                ? Task.FromResult(NewsResponse.FAILED("Fetcher is running"))
                : GetArticle(currentArticleKey);
        }
    }
}