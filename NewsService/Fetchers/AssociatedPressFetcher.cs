using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsService.Services;

namespace NewsService.Fetchers
{
    public class AssociatedPressFetcher : AbstractFetcher<AssociatedPressFetcher>, IFetcher
    {
        private const string NAME = "associated_press";

        public AssociatedPressFetcher(IConfiguration _configuration, ILoggerFactory _loggerFactory) : base(_configuration, NAME, _loggerFactory)
        {
        }

        public Task<IEnumerable<string>> Fetch(RedisCacheService _redisCacheService)
        {
            throw new NotImplementedException();
        }
    }
}