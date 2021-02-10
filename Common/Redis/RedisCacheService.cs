using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Common.Response;

using Microsoft.Extensions.Logging;

using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Common.Redis
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly ILogger logger;
        private readonly IRedisCacheClient redis;

        public RedisCacheService(ILoggerFactory _loggerFactory, IRedisCacheClient _redis)
        {
            logger = _loggerFactory.CreateLogger<RedisCacheService>();
            redis = _redis;
        }

        public async Task<IResponse> GetValue<T>(string _key)
        {
            if (!await redis.Db0.ExistsAsync(_key))
            {
                logger.LogDebug("Could not find object in Redis with key: {Key}", _key);

                return FailureResponse.Instance;
            }

            var response = await redis.Db0.GetAsync<T>(_key);
            logger.LogDebug("Returned object from Redis with key: {Key}", _key);

            return new RedisResponse<T>
            {
                Key = _key,
                Value = response
            };
        }

        public async Task<List<IResponse>> GetValues<T>(IEnumerable<string> _keys)
        {
            var keys = _keys.ToList();
            var redisResponse = await redis.Db0.GetAllAsync<T>(keys);
            logger.LogDebug("Returned objects from Redis with keys: {KeyCount}", keys.Count);

            var response = new List<IResponse>();

            foreach (var (key, value) in redisResponse)
            {
                response.Add(new RedisResponse<T>
                {
                    Key = key,
                    Value = value
                });
            }

            return response;
        }

        public Task<bool> AddValue<T>(string _key, T _value)
        {
            logger.LogDebug("Added object to Redis with key: {Key}", _key);

            return redis.Db0.AddAsync(_key, _value, TimeSpan.FromDays(2));
        }

        public async Task<List<string>> GetKeys(string _searchPattern)
        {
            var keys = await redis.Db0.SearchKeysAsync(_searchPattern);
            var result = keys.ToList();
            logger.LogDebug("Returned keys from Redis {Count}", result.Count);

            return result;
        }

        public async Task<bool> KeyExist(string _key)
        {
            var keys = await GetKeys(_key);
            return keys.Contains(_key);
        }
    }
}