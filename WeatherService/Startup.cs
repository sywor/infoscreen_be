using Common.Bootstrap;
using Common.Config;
using Common.Redis;

using Hangfire;
using Hangfire.Redis;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Newtonsoft;

using WeatherService.Smhi;

namespace WeatherService
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
            var bootstrapConfiguration = configuration.GetSection("Bootstrap").Get<BootstrapConfiguration>();

            _services.AddGrpc();
            _services.AddAuthorization();

            var redisCacheConnectionPoolManager = new RedisCacheConnectionPoolManager(redisConfiguration);

            _services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
            _services.AddSingleton<IRedisCacheConnectionPoolManager>(redisCacheConnectionPoolManager);
            _services.AddSingleton<ISerializer, NewtonsoftSerializer>();

            _services.AddSingleton((_provider) => _provider.GetRequiredService<IRedisCacheClient>().GetDbFromConfiguration());

            _services.AddSingleton<IRedisCacheService, RedisCacheService>();
            _services.AddSingleton(redisConfiguration);
            _services.AddSingleton(minioConfiguration);
            _services.AddSingleton(bootstrapConfiguration);

            _services.AddSingleton<IBootstrapService<SmhiFetcher>, BootstrapService<SmhiFetcher>>();

            _services.AddHangfire(_configuration =>
            {
                var connectionMultiplexer = (ConnectionMultiplexer) redisCacheConnectionPoolManager.GetConnection();
                var redisStorageOptions = new RedisStorageOptions
                {
                    Prefix = "hangfire_weather:"
                };

                _configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSerilogLogProvider()
                    .UseRedisStorage(connectionMultiplexer, redisStorageOptions);
            });
        }

        public void Configure(IApplicationBuilder _app, IWebHostEnvironment _env, IBootstrapService<SmhiFetcher> _bootstrap)
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
                _endpoints.MapGrpcService<Services.WeatherService>();
                _endpoints.MapHangfireDashboard();
                _endpoints.MapGet("/",
                                  async context =>
                                  {
                                      await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                                  });
            });

            _bootstrap.Launch();
        }
    }
}