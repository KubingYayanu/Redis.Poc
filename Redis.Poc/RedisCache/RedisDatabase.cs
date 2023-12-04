using System.Text.Json;
using System.Text.Json.Serialization;
using Redis.Poc.JsonConverters;
using StackExchange.Redis;

namespace Redis.Poc.RedisCache
{
    public class RedisDatabase : IRedisDatabase
    {
        private readonly ConnectionMultiplexer _connection;
        private readonly int _db;

        public RedisDatabase(ConnectionMultiplexer connection, int db)
        {
            _connection = connection;
            _db = db;
        }

        private IDatabase Database => _connection.GetDatabase(_db);

        public bool IsExist(string key)
        {
            return Database.KeyExists(key);
        }

        public async Task<bool> IsExistAsync(string key)
        {
            return await Database.KeyExistsAsync(key);
        }

        public bool Set<T>(string key, T value, TimeSpan? expiry = default)
        {
            var cacheData = Serialize(value);
            return Database.StringSet(key, cacheData, expiry);
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = default)
        {
            var cacheData = Serialize(value);
            return await Database.StringSetAsync(key, cacheData, expiry);
        }

        public T Get<T>(string key)
        {
            if (Database.KeyExists(key))
            {
                var value = Database.StringGet(key);
                return Deserialize<T>(value);
            }

            return default;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            if (await Database.KeyExistsAsync(key))
            {
                var value = await Database.StringGetAsync(key);
                return Deserialize<T>(value);
            }

            return default;
        }

        private static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, GetOptions());
        }

        private static T? Deserialize<T>(RedisValue value)
        {
            return JsonSerializer.Deserialize<T>(value, GetOptions());
        }

        private static JsonSerializerOptions GetOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new BsonDocumentConverter()
                },
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            };
        }
    }
}