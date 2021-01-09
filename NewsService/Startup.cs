using Hangfire;
using Hangfire.Redis;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NewsService.Config;
using NewsService.Fetchers;
using NewsService.Services;

using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace NewsService
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        public void ConfigureServices(IServiceCollection _services)
        {
            var redisConfiguration = configuration.GetSection("Redis").Get<RedisConfiguration>();
            var newsSourceConfigurations = new NewsSourceConfigurations(configuration.GetSection("NewsSources").Get<NewsSourceConfiguration[]>());
            var minioConfiguration = configuration.GetSection("Minio").Get<MinioConfiguration>();

            _services.AddGrpc();
            _services.AddAuthorization();

            var redisCacheConnectionPoolManager = new RedisCacheConnectionPoolManager(redisConfiguration);

            _services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
            _services.AddSingleton<IRedisCacheConnectionPoolManager>(redisCacheConnectionPoolManager);
            _services.AddSingleton<ISerializer, NewtonsoftSerializer>();

            _services.AddSingleton((_provider) => _provider.GetRequiredService<IRedisCacheClient>().GetDbFromConfiguration());

            _services.AddSingleton<NewsHandlerService>();
            _services.AddSingleton<RedisCacheService>();
            _services.AddSingleton(redisConfiguration);
            _services.AddSingleton(newsSourceConfigurations);
            _services.AddSingleton(minioConfiguration);

            _services.AddHangfire(_configuration =>
            {
                var connectionMultiplexer = (ConnectionMultiplexer) redisCacheConnectionPoolManager.GetConnection();
                var redisStorageOptions = new RedisStorageOptions
                {
                    Prefix = "hangfire:"
                };

                _configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSerilogLogProvider()
                    .UseRedisStorage(connectionMultiplexer, redisStorageOptions);
            });
        }

        public void Configure(IApplicationBuilder _app, IWebHostEnvironment _env)
        {
            if (_env.IsDevelopment())
            {
                _app.UseDeveloperExceptionPage();
            }

            _app.UseRouting();
            _app.UseAuthorization();
            _app.UseHangfireServer();
            _app.UseHangfireDashboard();

            _app.UseEndpoints(_endpoints =>
            {
                _endpoints.MapGrpcService<NewsGrpcService>();

                _endpoints.MapGet("/",
                                  async _context =>
                                  {
                                      await _context.Response.WriteAsync(
                                          "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                                  });

                _endpoints.MapHangfireDashboard();

#if DEBUG
                BackgroundJob.Enqueue<CnnFetcher>(_fetcher => _fetcher.Fetch());
#else
                RecurringJob.AddOrUpdate<CnnFetcher>(CnnFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
                RecurringJob.AddOrUpdate<ArsTechnicaFetcher>(ArsTechnicaFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
                RecurringJob.AddOrUpdate<AssociatedPressFetcher>(AssociatedPressFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
                RecurringJob.AddOrUpdate<BbcFetcher>(BbcFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
                RecurringJob.AddOrUpdate<CnbcFetcher>(CnbcFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
                RecurringJob.AddOrUpdate<EngadgetFetcher>(EngadgetFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
                RecurringJob.AddOrUpdate<IgnFetcher>(IgnFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
                RecurringJob.AddOrUpdate<MashableFetcher>(MashableFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
                RecurringJob.AddOrUpdate<NationalGeographicFetcher>(NationalGeographicFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
                RecurringJob.AddOrUpdate<NyTimesFetcher>(NyTimesFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
                RecurringJob.AddOrUpdate<PolygonFetcher>(PolygonFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
                RecurringJob.AddOrUpdate<RecodeFetcher>(RecodeFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
                RecurringJob.AddOrUpdate<ReutersFetcher>(ReutersFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
                RecurringJob.AddOrUpdate<TechradarFetcher>(TechradarFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
                RecurringJob.AddOrUpdate<TheVergeFetcher>(TheVergeFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
                RecurringJob.AddOrUpdate<ViceFetcher>(ViceFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
                RecurringJob.AddOrUpdate<WashingtonPostFetcher>(WashingtonPostFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
                RecurringJob.AddOrUpdate<WiredFetcher>(WiredFetcher.NAME, _fetcher => _fetcher.Fetch(), Cron.Hourly);
#endif
            });
        }
    }
}