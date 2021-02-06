using System.Collections.Generic;
using System.Threading.Tasks;

namespace WeatherService.Services
{
    public interface IRedisCacheService
    {
        Task<WeatherResponse?> GetValue(string _key);
        Task<List<WeatherResponse>> GetValues(IEnumerable<string> _keys);
        Task<bool> AddValue(string _key, WeatherResponse _value);
        Task<List<string>> GetKeys(string _searchPattern);
        Task<bool> KeyExist(string _key);
    }
}