using Common.Response;

namespace Common.Redis
{
    public readonly struct RedisResponse<T> : IResponse
    {
        public bool Success => true;

        public string Key { get; init; }
        public T Value { get; init; }
    }
}