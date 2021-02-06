using System;

using Common;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace WeatherService
{
    public class Program
    {
        public static void Main(string[] _args)
        {
            Log.Logger = new LoggerConfiguration()
                         .MinimumLevel.Information()
                         .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                         .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext:l}] {Message:lj} {NewLine}{Exception}")
                         .Enrich.With(new SimpleClassEnricher())
                         .Enrich.FromLogContext()
                         .Enrich.WithExceptionDetails()
                         .CreateLogger();
            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(_args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] _args) =>
            Host.CreateDefaultBuilder(_args)
                .UseSerilog()
                .ConfigureWebHostDefaults(_webBuilder =>
                {
                    _webBuilder.UseStartup<Startup>()
                               .UseSerilog();
                });
    }
}