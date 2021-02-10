using System;
using System.Diagnostics;
using System.Threading;

using Common.Config;

using Microsoft.Extensions.Logging;

namespace Common.Recurrence
{
    public class RecurrenceService<T> where T : IRunnable
    {
        private readonly RecurrenceConfiguration configuration;
        private readonly T runnable;
        private readonly ILogger<RecurrenceService<T>> logger;
        private Timer timer;

        public DateTime LastUpdate { get; private set; } = DateTime.MinValue;

        public RecurrenceService(ILoggerFactory _loggerFactory, RecurrenceConfiguration _configuration, T _runnable)
        {
            logger = _loggerFactory.CreateLogger<RecurrenceService<T>>();
            configuration = _configuration;
            runnable = _runnable;
        }

        public void Start()
        {
            timer = new Timer(async _ =>
            {
                logger.LogInformation("Executing {Type}", nameof(T));
                var time = Stopwatch.StartNew();

                await runnable.Run();

                time.Stop();
                logger.LogInformation("Done executing {Type}. Took: {Took}", nameof(T), time.Elapsed);
                LastUpdate = DateTime.UtcNow;

            }, null, TimeSpan.Zero, configuration.Period);
        }
    }
}