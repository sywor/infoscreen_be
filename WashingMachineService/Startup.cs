using Common.Config;
using Common.Recurrence;
using Common.Redis;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Newtonsoft.Json;

using NodaTime;
using NodaTime.Serialization.JsonNet;

using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Newtonsoft;

using WashingMachineService.WashingMachine;

namespace WashingMachineService
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
            var recurrenceConfiguration = configuration.GetSection("Recurrence").Get<RecurrenceConfiguration>();

            _services.AddAuthorization();
            _services.AddGrpc();

            var redisCacheConnectionPoolManager = new RedisCacheConnectionPoolManager(redisConfiguration);

            _services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
            _services.AddSingleton<IRedisCacheConnectionPoolManager>(redisCacheConnectionPoolManager);

            var settings = new JsonSerializerSettings();
            settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            var jsonSerializer = new NewtonsoftSerializer(settings);

            _services.AddSingleton<ISerializer>(jsonSerializer);

            _services.AddSingleton(_provider => _provider.GetRequiredService<IRedisCacheClient>().GetDbFromConfiguration());

            _services.AddSingleton<IRedisCacheService, RedisCacheService>();
            _services.AddSingleton(redisConfiguration);

            _services.AddSingleton(recurrenceConfiguration);

            _services.AddSingleton<HomeConnectFetcher>();
            _services.AddSingleton<RecurrenceService<HomeConnectFetcher>>();
        }

        public void Configure(IApplicationBuilder _app, IWebHostEnvironment _env, RecurrenceService<HomeConnectFetcher> _recurrence)
        {
            if (_env.IsDevelopment())
            {
                _app.UseDeveloperExceptionPage();
            }

            _app.UseRouting();

            _app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<Services.WashingMachineService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            _recurrence.Start();
        }
    }
}