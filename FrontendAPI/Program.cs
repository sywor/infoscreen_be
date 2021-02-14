using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FrontendAPI
{
    public class Program
    {
        public static void Main(string[] _args)
        {
            CreateHostBuilder(_args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] _args) =>
            Host.CreateDefaultBuilder(_args)
                .ConfigureWebHostDefaults(_webBuilder => { _webBuilder.UseStartup<Startup>(); });
    }
}