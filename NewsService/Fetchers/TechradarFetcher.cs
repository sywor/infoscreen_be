using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsService.Services;

namespace NewsService.Fetchers
{
    public class TechradarFetcher : AbstractFetcher<TechradarFetcher>, IFetcher
    {
        private const string NAME = "techradar";

        public TechradarFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }

        public Task<IEnumerable<string>> Fetch(RedisCacheService _redisCacheService)
        {
            throw new NotImplementedException();
        }
    }
}