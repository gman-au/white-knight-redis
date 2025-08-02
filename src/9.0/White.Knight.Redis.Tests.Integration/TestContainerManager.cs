using System;
using System.Threading.Tasks;
using Docker.DotNet;
using Testcontainers.Redis;

namespace White.Knight.Redis.Tests.Integration
{
    public class TestContainerManager : ITestContainerManager
    {
        private readonly RedisBuilder _redisBuilder;
        private RedisContainer _redisContainer;

        public TestContainerManager()
        {
            _redisBuilder =
                new RedisBuilder()
                    .WithImage("redis/redis-stack:latest")
                    .WithName("redis-test-harness")
                    .WithPortBinding(6379, 6379);
        }

        public async Task StartAsync()
        {
            try
            {
                /*
                 */
                _redisContainer =
                    _redisBuilder
                        .Build();

                _redisContainer
                    .StartAsync()
                    .Wait();
                /*
             */
            }
            catch (AggregateException ax)
            {
                if (ax.InnerException is DockerApiException dockerApiException)
                    if (dockerApiException.Message.Contains("Conflict"))
                        return;

                throw;
            }
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
            }
        }
    }
}