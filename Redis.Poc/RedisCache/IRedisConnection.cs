using RedLockNet.SERedis;
using StackExchange.Redis;

namespace Redis.Poc.RedisCache
{
    public interface IRedisConnection
    {
        IDatabase GetDatabase(int db = 0);

        RedLockFactory RedisLockFactory { get; }
    }
}