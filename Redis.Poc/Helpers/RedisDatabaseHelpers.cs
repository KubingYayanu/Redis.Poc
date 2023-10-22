using System.Text.Json;
using System.Text.Json.Serialization;
using Redis.Poc.JsonConverters;
using StackExchange.Redis;

namespace Redis.Poc.Helpers
{
    public static class RedisDatabaseHelpers
    {
        public static bool IsExist(this IDatabase db, string key)
        {
            return db.KeyExists(key);
        }

        public static async Task<bool> IsExistAsync(this IDatabase db, string key)
        {
            return await db.KeyExistsAsync(key);
        }

        public static bool Set<T>(
            this IDatabase db,
            string key,
            T value,
            TimeSpan? expiry = default)
        {
            var cacheData = Serialize(value);
            return db.StringSet(key, cacheData, expiry);
        }

        public static async Task<bool> SetAsync<T>(
            this IDatabase db,
            string key,
            T value,
            TimeSpan? expiry = default)
        {
            var cacheData = Serialize(value);
            return await db.StringSetAsync(key, cacheData, expiry);
        }

        public static T Get<T>(
            this IDatabase db,
            string key)
        {
            if (db.IsExist(key))
            {
                var value = db.StringGet(key);
                return Deserialize<T>(value);
            }

            return default;
        }

        public static async Task<T> GetAsync<T>(
            this IDatabase db,
            string key)
        {
            if (await db.IsExistAsync(key))
            {
                var value = await db.StringGetAsync(key);
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