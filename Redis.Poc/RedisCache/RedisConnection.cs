using Microsoft.Extensions.Configuration;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Redis.Poc.RedisCache
{
    public class RedisConnection : IRedisConnection
    {
        private const string ConnectionStringSection = "Redis:ConnectionString";

        private readonly IConfiguration _config;
        private readonly Lazy<ConnectionMultiplexer> _connection;

        public RedisConnection(IConfiguration config)
        {
            _config = config;

            var connectionString = _config.GetValue<string>(ConnectionStringSection);
            _connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connectionString));
        }

        private ConnectionMultiplexer Connection => _connection.Value;

        public IDatabase GetDatabase(int db = 0)
        {
            if (db < -1 || 15 < db)
            {
                throw new ArgumentException("Redis Database 設定為 0 ~ 15");
            }
            
            return Connection.GetDatabase(db);
        }

        public RedLockFactory RedisLockFactory
        {
            get
            {
                var multiplexers = new List<RedLockMultiplexer> { Connection };
                return RedLockFactory.Create(multiplexers);
            }
        }
    }
}