using Microsoft.Extensions.Logging;
using Redis.Poc.RedisCache;

namespace Redis.Poc.Services
{
    public class ConcurrencyService : IConcurrencyService
    {
        private const string Key = "concurrency:int_increase";
        private const string KeyWithLock = "concurrency:int_increase_lock";
        private const string KeyWithLockRetry = "concurrency:int_increase_lock_retry";

        private readonly IRedisConnection _redisConnection;
        private readonly ILogger<ConcurrencyService> _logger;

        public ConcurrencyService(
            IRedisConnection redisConnection,
            ILogger<ConcurrencyService> logger)
        {
            _redisConnection = redisConnection;
            _logger = logger;
        }

        private IRedisDatabase RedisDatabase => _redisConnection.GetDatabase();

        public async Task Run()
        {
            try
            {
                var expiry = TimeSpan.FromSeconds(30);

                await RedisDatabase.SetAsync(Key, 0, expiry);
                await Increase();

                await RedisDatabase.SetAsync(KeyWithLock, 0, expiry);
                await IncreaseWithLock();

                await RedisDatabase.SetAsync(KeyWithLockRetry, 0, expiry);
                await IncreaseWithLockRetry();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Boom!");
            }

            await Task.CompletedTask;
        }

        private async Task Increase()
        {
            var expiry = TimeSpan.FromSeconds(30);
            var tasks = new List<Task>();
            for (int i = 0; i < 50; i++)
            {
                var task = Task.Run(async () =>
                {
                    var value = await RedisDatabase.GetAsync<int>(Key);
                    await RedisDatabase.SetAsync(Key, ++value, expiry);

                    _logger.LogInformation(
                        message: "Thread: {Thread}, Key: {Key}. Set value: {Value}",
                        args: new object[] { Thread.CurrentThread.ManagedThreadId, Key, value });
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private async Task IncreaseWithLock()
        {
            var expiry = TimeSpan.FromSeconds(30);
            var tasks = new List<Task>();
            for (int i = 0; i < 50; i++)
            {
                var task = Task.Run(async () =>
                {
                    using (var redLock = await _redisConnection.RedisLockFactory.CreateLockAsync(
                               resource: KeyWithLock,
                               expiryTime: expiry))
                    {
                        if (redLock.IsAcquired)
                        {
                            _logger.LogInformation(
                                message: "Thread: {Thread}, Key: {Key}. Lock start at {Now}",
                                args: new object[] { Thread.CurrentThread.ManagedThreadId, KeyWithLock, DateTime.Now });

                            var value = await RedisDatabase.GetAsync<int>(KeyWithLock);
                            await RedisDatabase.SetAsync(KeyWithLock, ++value, expiry);

                            _logger.LogInformation(
                                message: "Thread: {Thread}, Key: {Key}. Set value: {Value}",
                                args: new object[] { Thread.CurrentThread.ManagedThreadId, KeyWithLock, value });

                            _logger.LogInformation(
                                message: "Thread: {Thread}, Key: {Key}. Lock end at {Now}",
                                args: new object[] { Thread.CurrentThread.ManagedThreadId, KeyWithLock, DateTime.Now });
                        }
                        else
                        {
                            _logger.LogInformation(
                                message: "Thread: {Thread}, Key: {Key}. Not get the locker",
                                args: new object[] { Thread.CurrentThread.ManagedThreadId, KeyWithLock });
                        }
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private async Task IncreaseWithLockRetry()
        {
            var expiry = TimeSpan.FromSeconds(30);
            var wait = TimeSpan.FromSeconds(10);
            var retry = TimeSpan.FromSeconds(1);
            var tasks = new List<Task>();
            for (int i = 0; i < 50; i++)
            {
                var task = Task.Run(async () =>
                {
                    using (var redLock =
                           await _redisConnection.RedisLockFactory.CreateLockAsync(
                               resource: KeyWithLockRetry,
                               expiryTime: expiry,
                               waitTime: wait,
                               retryTime: retry))
                    {
                        if (redLock.IsAcquired)
                        {
                            _logger.LogInformation(
                                message: "Thread: {Thread}, Key: {Key}. Lock start at {Now}",
                                args: new object[]
                                    { Thread.CurrentThread.ManagedThreadId, KeyWithLockRetry, DateTime.Now });

                            var value = await RedisDatabase.GetAsync<int>(KeyWithLockRetry);
                            await RedisDatabase.SetAsync(KeyWithLockRetry, ++value, expiry);

                            _logger.LogInformation(
                                message: "Thread: {Thread}, Key: {Key}. Set value: {Value}",
                                args: new object[] { Thread.CurrentThread.ManagedThreadId, KeyWithLockRetry, value });

                            _logger.LogInformation(
                                message: "Thread: {Thread}, Key: {Key}. Lock end at {Now}",
                                args: new object[]
                                    { Thread.CurrentThread.ManagedThreadId, KeyWithLockRetry, DateTime.Now });
                        }
                        else
                        {
                            _logger.LogInformation(
                                message: "Thread: {Thread}, Key: {Key}. Not get the locker",
                                args: new object[] { Thread.CurrentThread.ManagedThreadId, KeyWithLockRetry });
                        }
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }
    }
}