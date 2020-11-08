using System.Collections.Generic;
using System.Threading.Tasks;
using NewsService.Services;

namespace NewsService.Fetchers
{
    public interface IFetcher
    {
        Task<IEnumerable<string>> Fetch(RedisCacheService _redisCacheService);
    }
}