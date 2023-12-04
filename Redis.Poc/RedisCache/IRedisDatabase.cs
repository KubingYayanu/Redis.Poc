namespace Redis.Poc.RedisCache
{
    public interface IRedisDatabase
    {
        bool IsExist(string key);

        Task<bool> IsExistAsync(string key);

        bool Set<T>(string key, T value, TimeSpan? expiry = default);

        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = default);

        T Get<T>(string key);

        Task<T> GetAsync<T>(string key);
    }
}