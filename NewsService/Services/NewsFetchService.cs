using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NewsService.Fetchers;

namespace NewsService.Services
{
    public class NewsFetchService : IHostedService, IDisposable
    {
        private readonly ILogger logger;
        private readonly RedisCacheService redis;
        private readonly List<IFetcher> fetchers = new List<IFetcher>();
        private Timer timer;

        public DateTime LastUpdate { get; private set; } = DateTime.MinValue;

        public NewsFetchService(ILoggerFactory _loggerFactory, IConfiguration _configuration, RedisCacheService _redis)
        {
            logger = _loggerFactory.CreateLogger<NewsFetchService>();
            redis = _redis;

            // fetchers.Add(new ArsTechnicaFetcher(_configuration, _loggerFactory));
            // fetchers.Add(new AssociatedPressFetcher(_configuration, _loggerFactory));
            fetchers.Add(new BbcFetcher(_configuration, _loggerFactory));
            // fetchers.Add(new CnbcFetcher(_configuration, _loggerFactory));
            // fetchers.Add(new CnnFetcher(_configuration, _loggerFactory));
            // fetchers.Add(new EngadgetFetcher(_configuration, _loggerFactory));
            // fetchers.Add(new IgnFetcher(_configuration, _loggerFactory));
            // fetchers.Add(new MashableFetcher(_configuration, _loggerFactory));
            // fetchers.Add(new NationalGeographicFetcher(_configuration, _loggerFactory));
            // fetchers.Add(new NyTimesFetcher(_configuration, _loggerFactory));
            // fetchers.Add(new PolygonFetcher(_configuration, _loggerFactory));
            // fetchers.Add(new RecodeFetcher(_configuration, _loggerFactory));
            // fetchers.Add(new ReutersFetcher(_configuration, _loggerFactory));
            // fetchers.Add(new TechradarFetcher(_configuration, _loggerFactory));
            // fetchers.Add(new TheVergeFetcher(_configuration, _loggerFactory));
            // fetchers.Add(new ViceFetcher(_configuration, _loggerFactory));
            // fetchers.Add(new WashingtonPostFetcher(_configuration, _loggerFactory));
            // fetchers.Add(new WiredFetcher(_configuration, _loggerFactory));
        }

        public Task StartAsync(CancellationToken _cancellationToken)
        {
            timer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken _cancellationToken)
        {
            timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private async void TimerCallback(object? _state)
        {
            var time = Stopwatch.StartNew();
            logger.LogInformation("Fetching news");

            var tasks = fetchers.Select(_fetcher => _fetcher.Fetch(redis));
            var list = (await Task.WhenAll(tasks.ToArray())).SelectMany(_x => _x).ToList();

            time.Stop();
            logger.LogInformation("Done fetching news. Took: {Took} and fetched {Count} articles", time.Elapsed, list.Count);
            LastUpdate = DateTime.UtcNow;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}