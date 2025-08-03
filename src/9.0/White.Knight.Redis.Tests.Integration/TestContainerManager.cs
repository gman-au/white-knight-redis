using System;
using System.Linq;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Testcontainers.Redis;
using White.Knight.Redis.Tests.Integration.Extensions;
using Xunit.Abstractions;

namespace White.Knight.Redis.Tests.Integration
{
    public class TestContainerManager : ITestContainerManager
    {
        private const int RedisPort = 6379;

        private readonly RedisBuilder _redisBuilder;
        private RedisContainer _redisContainer;
        private readonly ITestOutputHelper _helper;

        public TestContainerManager(ITestOutputHelper helper)
        {
            _redisBuilder =
                new RedisBuilder()
                    .WithImage("redis/redis-stack:latest")
                    .WithName($"redis-test-harness-{Guid.NewGuid()}")
                    .WithPortBinding(RedisPort, RedisPort)
                    .WithEnvironment("REDIS_ARGS", "--save '' --appendonly no")
                    .WithTmpfsMount($"/data/{Guid.NewGuid()}") // Mount data directory in memory
                    .WithWaitStrategy(
                        Wait
                            .ForUnixContainer()
                            .UntilPortIsAvailable(RedisPort)
                            .UntilMessageIsLogged("Ready to accept connections")
                    );

            _helper = helper;
        }

        public async Task StartAsync()
        {
            /*
                 */
            _redisContainer =
                _redisBuilder
                    .Build();

            await
                _redisContainer
                    .StartAsync();
            /*
             */

            _helper.WriteLine($"Started container: {_redisContainer.Name}");
            _helper.WriteLine($"Container ID: {_redisContainer.Id}");
            _helper.WriteLine($"Connection: {_redisContainer.GetConnectionString()}");
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

                await
                    RedisPort
                        .WaitForPortToBeReleased(_helper, TimeSpan.FromSeconds(20));

                /*do
                {
                    await Task.Delay(5000);
                } while (_redisContainer.State != TestcontainersStates.Undefined);*/
            }
        }
    }
}