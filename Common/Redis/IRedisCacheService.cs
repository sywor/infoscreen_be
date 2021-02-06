using System.Collections.Generic;
using System.Threading.Tasks;

using Common.Response;

namespace Common.Redis
{
    public interface IRedisCacheService
    {
        Task<IResponse> GetValue<T>(string _key);
        Task<List<IResponse>> GetValues<T>(IEnumerable<string> _keys);
        Task<bool> AddValue<T>(string _key, T _value);
        Task<List<string>> GetKeys(string _searchPattern);
        Task<bool> KeyExist(string _key);
    }
}