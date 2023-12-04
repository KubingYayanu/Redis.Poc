using RedLockNet;

namespace Redis.Poc.RedisCache
{
    public interface IRedisConnection
    {
        IRedisDatabase GetDatabase(int db = 0);

        IDistributedLockFactory RedisLockFactory { get; }
    }
}