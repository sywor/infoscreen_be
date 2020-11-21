using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewsService.Services;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace NewsService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection _services)
        {
            var redisConfiguration = Configuration.GetSection("Redis").Get<RedisConfiguration>();

            _services.AddGrpc();

            _services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
            _services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
            _services.AddSingleton<ISerializer, NewtonsoftSerializer>();
            _services.AddSingleton(redisConfiguration);

            _services.AddSingleton((_provider) => _provider.GetRequiredService<IRedisCacheClient>().GetDbFromConfiguration());

            _services.AddSingleton<NewsHandlerService>();
            _services.AddSingleton<RedisCacheService>();
            _services.AddHostedService<NewsFetchService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder _app, IWebHostEnvironment _env)
        {
            if (_env.IsDevelopment())
            {
                _app.UseDeveloperExceptionPage();
            }

            _app.UseRouting();

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
        }
    }
}