using Microsoft.Extensions.Configuration;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Redis.Poc.RedisCache
{
    public class RedisConnection : IRedisConnection
    {
        private const string ConnectionStringSection = "Redis:ConnectionString";

        private readonly Lazy<ConnectionMultiplexer> _connection;
        private readonly IConfiguration _config;

        public RedisConnection(IConfiguration config)
        {
            _config = config;
            _connection = new Lazy<ConnectionMultiplexer>(() =>
            {
                var option = GetConfigurationOptions();
                return ConnectionMultiplexer.Connect(option);
            });
        }

        private ConfigurationOptions GetConfigurationOptions()
        {
            var connectionString = _config.GetValue<string>(ConnectionStringSection);
            var option = new ConfigurationOptions
            {
                EndPoints = { connectionString }
            };
            return option;
        }

        private ConnectionMultiplexer Connection => _connection.Value;

        public IRedisDatabase GetDatabase(int db = 0)
        {
            if (db < -1
                || 15 < db)
            {
                throw new ArgumentException("Redis Database 設定為 0 ~ 15");
            }

            return new RedisDatabase(Connection, db);
        }

        public IDistributedLockFactory RedisLockFactory
        {
            get
            {
                var multiplexers = new List<RedLockMultiplexer> { Connection };
                return RedLockFactory.Create(multiplexers);
            }
        }
    }
}