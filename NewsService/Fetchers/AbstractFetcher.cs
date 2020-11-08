using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NewsService.Fetchers
{
    public abstract class AbstractFetcher<T>
    {
        protected readonly Uri uri;
        protected readonly ILogger logger;

        public AbstractFetcher(IConfiguration _configuration, string _name, ILoggerFactory _loggerFactory)
        {
            var uriString = _configuration.GetSection("NewsSources")
                .GetChildren()
                .SingleOrDefault(_x => _x["Name"].Equals(_name))?["Url"];

            if (uriString == null)
            {
                throw new InvalidEnumArgumentException();
            }

            uri = new Uri(uriString);
            logger = _loggerFactory.CreateLogger<T>();
        }
    }
}