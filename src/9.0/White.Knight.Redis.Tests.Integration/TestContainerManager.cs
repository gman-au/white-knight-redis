using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using Testcontainers.Redis;

namespace White.Knight.Redis.Tests.Integration
{
    public class TestContainerManager : ITestContainerManager
    {
        private RedisContainer _redisContainer;

        private static RedisBuilder GetRedisBuilder(int hostedPort)
        {
            return
                new RedisBuilder()
                    .WithImage("redis/redis-stack:latest")
                    .WithName($"redis-test-harness-{Guid.NewGuid()}")
                    .WithPortBinding(hostedPort, 6379)
                .WithWaitStrategy(
                    Wait
                        .ForUnixContainer()
                        .UntilPortIsAvailable(6379)
                       // .UntilMessageIsLogged("Ready to accept connections")
                );
        }

        public async Task StartAsync(int hostedPort)
        {
            _redisContainer =
                GetRedisBuilder(hostedPort)
                    .Build();

            await
                _redisContainer
                    .StartAsync();
        }

        public async Task StopAsync()
        {
            if (_redisContainer != null)
            {
                await
                    _redisContainer
                        .StopAsync();

                await
                    _redisContainer
                        .DisposeAsync();

                _redisContainer = null;
            }
        }
    }
}