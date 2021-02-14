using Common.Config;

using FrontendAPI.Config;
using FrontendAPI.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace FrontendAPI
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection _services)
        {
            var newsGrpcServiceConfiguration = configuration.GetSection("NewsGrpcSettings").Get<NewsGrpcServiceConfiguration>();
            var weatherServiceConfiguration = configuration.GetSection("WeatherSettings").Get<WeatherServiceConfiguration>();
            var minioConfiguration = configuration.GetSection("Minio").Get<MinioConfiguration>();

            _services.AddSingleton(newsGrpcServiceConfiguration);
            _services.AddSingleton(weatherServiceConfiguration);
            _services.AddSingleton(minioConfiguration);
            _services.AddSingleton<NewsGrpcService>();
            _services.AddSingleton<WeatherGrpcService>();
            _services.AddSingleton<ResourceMinioService>();

            _services.AddControllers();
            _services.AddSwaggerGen(_c => { _c.SwaggerDoc("v1", new OpenApiInfo {Title = "FrontendAPI", Version = "v1"}); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder _app, IWebHostEnvironment _env)
        {
            if (_env.IsDevelopment())
            {
                _app.UseDeveloperExceptionPage();
                _app.UseSwagger();
                _app.UseSwaggerUI(_c => _c.SwaggerEndpoint("/swagger/v1/swagger.json", "FrontendAPI v1"));
            }

            _app.UseHttpsRedirection();

            _app.UseRouting();

            _app.UseAuthorization();

            _app.UseEndpoints(_endpoints => { _endpoints.MapControllers(); });
        }
    }
}