using Microsoft.Extensions.Logging;
using Redis.Poc.Helpers;
using Redis.Poc.Models;
using Redis.Poc.RedisCache;

namespace Redis.Poc.Services
{
    public class ConcurrencyService : IConcurrencyService
    {
        private readonly IRedisConnection _redisConnection;
        private readonly ILogger<ConcurrencyService> _logger;

        public ConcurrencyService(
            IRedisConnection redisConnection,
            ILogger<ConcurrencyService> logger)
        {
            _redisConnection = redisConnection;
            _logger = logger;
        }

        public async Task Run()
        {
            var db = _redisConnection.GetDatabase();

            var key = "test:lucy";
            var model = new TestModel
            {
                Name = "Lucy",
                Age = 13
            };

            await db.SetAsync(key, model);

            var value = await db.GetAsync<TestModel>(key);

            _logger.LogInformation("Value: {@Value}", value);
        }
    }
}