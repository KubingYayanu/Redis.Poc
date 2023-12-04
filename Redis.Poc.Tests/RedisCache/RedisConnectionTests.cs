using NSubstitute;
using Redis.Poc.RedisCache;
using RedLockNet;

namespace Redis.Poc.Tests.RedisCache
{
    [TestFixture]
    public class RedisConnectionTests
    {
        private IRedisConnection _connection;

        [Test]
        public void RedisLockFactory_CreateLock_ReturnIsAcquired()
        {
            var redLock = _connection.RedisLockFactory.CreateLock(
                resource: "key",
                expiryTime: TimeSpan.FromMilliseconds(100));

            Assert.That(redLock.IsAcquired, Is.True);
        }

        [Test]
        public void RedisDatabase_IsExist_ReturnTrue()
        {
            var db = _connection.GetDatabase();
            var isExist = db.IsExist("key");

            Assert.That(isExist, Is.True);
        }

        [Test]
        public void RedisDatabase_Get_ReturnModel()
        {
            var db = _connection.GetDatabase();
            var model = db.Get<RedisModel>("key");

            Assert.That(model, Is.Not.Null);
            Assert.That(model.Name, Is.EqualTo("Hi"));
            Assert.That(model.Age, Is.EqualTo(13));
        }

        [Test]
        public void RedisDatabase_Set_DataSetAndReturnModel()
        {
            var db = _connection.GetDatabase();
            var isSet = db.Set("key", new RedisModel(), TimeSpan.FromMilliseconds(100));

            Assert.That(isSet, Is.True);

            db.Received(1).Set(Arg.Any<string>(), Arg.Any<RedisModel>(), Arg.Any<TimeSpan?>());
        }

        [Test]
        [SetUp]
        public void SetUp()
        {
            _connection = Substitute.For<IRedisConnection>();
            _connection.GetDatabase(Arg.Any<int>())
                .Returns(x => FakeRedisDatabase());

            _connection.RedisLockFactory
                .Returns(x => FakeRedisLockFactory());
        }

        private IRedisDatabase FakeRedisDatabase()
        {
            var db = Substitute.For<IRedisDatabase>();
            db.IsExist(Arg.Any<string>())
                .Returns(x => true);

            db.Get<RedisModel>(Arg.Any<string>())
                .Returns(x => new RedisModel { Name = "Hi", Age = 13 });

            db.Set(Arg.Any<string>(),
                    Arg.Any<RedisModel>(),
                    Arg.Any<TimeSpan?>())
                .Returns(x => true);

            return db;
        }

        private IDistributedLockFactory FakeRedisLockFactory()
        {
            var redLock = Substitute.For<IRedLock>();
            redLock.IsAcquired.Returns(x => true);

            var factory = Substitute.For<IDistributedLockFactory>();
            factory.CreateLock(
                    Arg.Any<string>(),
                    Arg.Any<TimeSpan>())
                .Returns(x => redLock);

            return factory;
        }

        public class RedisModel
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }
    }
}