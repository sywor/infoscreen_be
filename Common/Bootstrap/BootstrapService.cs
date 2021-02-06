using System.Threading.Tasks;

using Common.Config;

using Hangfire;
using Hangfire.Storage;

using Microsoft.Extensions.Logging;

namespace Common.Bootstrap
{
    public class BootstrapService<T> : IBootstrapService<T> where T : IRunnable
    {
        private readonly BootstrapConfiguration configuration;
        private readonly ILogger logger;

        public BootstrapService(ILoggerFactory _loggerFactory, BootstrapConfiguration _configuration)
        {
            logger = _loggerFactory.CreateLogger<BootstrapService<T>>();
            configuration = _configuration;
        }

        public void Launch()
        {
            BackgroundJob.Enqueue<T>(_fetcher => _fetcher.Run());
            RecurringJob.AddOrUpdate<T>(_fetcher => _fetcher.Run(), configuration.Cron);
        }
    }
}