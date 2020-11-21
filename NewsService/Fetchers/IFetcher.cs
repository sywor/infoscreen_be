using System.Collections.Generic;
using System.Threading.Tasks;
using NewsService.Data;
using NewsService.Services;

namespace NewsService.Fetchers
{
    public interface IFetcher
    {
        Task<IEnumerable<PageResult>> Fetch(RedisCacheService _redis);
    }
}