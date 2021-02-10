using Common.Config;
using Common.Recurrence;
using Common.Redis;

using FeedlySharp.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NewsService.Feedly;
using NewsService.Services;

using Newtonsoft.Json;

using NodaTime;
using NodaTime.Serialization.JsonNet;

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
            var minioConfiguration = configuration.GetSection("Minio").Get<MinioConfiguration>();
            var feedlyOptions = configuration.GetSection("Feedly").Get<FeedlyOptions>();
            var recurrenceConfiguration = configuration.GetSection("Bootstrap").Get<RecurrenceConfiguration>();

            _services.AddGrpc();
            _services.AddAuthorization();

            var redisCacheConnectionPoolManager = new RedisCacheConnectionPoolManager(redisConfiguration);

            _services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
            _services.AddSingleton<IRedisCacheConnectionPoolManager>(redisCacheConnectionPoolManager);

            var settings = new JsonSerializerSettings();
            settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            var jsonSerializer = new NewtonsoftSerializer(settings);

            _services.AddSingleton<ISerializer>(jsonSerializer);

            _services.AddSingleton((_provider) => _provider.GetRequiredService<IRedisCacheClient>().GetDbFromConfiguration());

            _services.AddSingleton<NewsHandlerService>();
            _services.AddSingleton<RedisCacheService>();
            _services.AddSingleton(redisConfiguration);
            _services.AddSingleton(minioConfiguration);
            _services.AddSingleton(feedlyOptions);
            _services.AddSingleton(recurrenceConfiguration);
            
            _services.AddSingleton<FeedlyFetcher>();
            _services.AddSingleton<RecurrenceService<FeedlyFetcher>>();
        }

        public void Configure(IApplicationBuilder _app, IWebHostEnvironment _env, RecurrenceService<FeedlyFetcher> _bootstrap)
        {
            if (_env.IsDevelopment())
            {
                _app.UseDeveloperExceptionPage();
            }

            _app.UseRouting();
            _app.UseAuthorization();
            _app.UseEndpoints(_endpoints =>
            {
                _endpoints.MapGrpcService<NewsGrpcService>();

                _endpoints.MapGet("/",
                                  async _context =>
                                  {
                                      await _context.Response.WriteAsync(
                                          "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                                  });
            });

            _bootstrap.Start();
        }
    }
}