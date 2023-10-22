using Microsoft.Extensions.Logging;
using Redis.Poc.Helpers;
using Redis.Poc.RedisCache;
using StackExchange.Redis;

namespace Redis.Poc.Services
{
    public class ConcurrencyService : IConcurrencyService
    {
        private const string Key = "concurrency:int_increase";

        private readonly IRedisConnection _redisConnection;
        private readonly ILogger<ConcurrencyService> _logger;

        public ConcurrencyService(
            IRedisConnection redisConnection,
            ILogger<ConcurrencyService> logger)
        {
            _redisConnection = redisConnection;
            _logger = logger;
        }

        private IDatabase RedisDatabase => _redisConnection.GetDatabase();

        public async Task Run()
        {
            try
            {
                await RedisDatabase.SetAsync(Key, 0);

                Parallel.For(0, 50, Increase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Boom!");
            }

            await Task.CompletedTask;
        }

        private void Increase(int x)
        {
            var expiry = TimeSpan.FromSeconds(30);
            var wait = TimeSpan.FromSeconds(10);
            var retry = TimeSpan.FromSeconds(1);

            using (var redLock = _redisConnection.RedisLockFactory.CreateLock(Key, expiry))
            {
                if (redLock.IsAcquired)
                {
                    _logger.LogInformation(
                        message: "Thread: {Thread}, Key: {Key}. Lock start at {Now}",
                        args: new object[] { Thread.CurrentThread.ManagedThreadId, Key, DateTime.Now });

                    var value = RedisDatabase.Get<int>(Key);
                    RedisDatabase.Set(Key, ++value);

                    _logger.LogInformation(
                        message: "Thread: {Thread}, Key: {Key}. Set value: {Value}",
                        args: new object[] { Thread.CurrentThread.ManagedThreadId, Key, value });

                    _logger.LogInformation(
                        message: "Thread: {Thread}, Key: {Key}. Lock end at {Now}",
                        args: new object[] { Thread.CurrentThread.ManagedThreadId, Key, DateTime.Now });
                }
                else
                {
                    _logger.LogInformation(
                        message: "Thread: {Thread}, Key: {Key}. Not get the locker",
                        args: new object[] { Thread.CurrentThread.ManagedThreadId, Key });
                }
            }
        }
    }
}