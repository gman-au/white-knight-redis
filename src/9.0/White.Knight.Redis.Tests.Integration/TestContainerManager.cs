using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using Testcontainers.Redis;
using Xunit.Abstractions;

namespace White.Knight.Redis.Tests.Integration
{
    public class TestContainerManager(ITestOutputHelper helper) : ITestContainerManager
    {
        private RedisContainer _redisContainer;

        private static RedisBuilder GetRedisBuilder(int hostedPort)
        {
            return
                new RedisBuilder()
                    .WithImage("redis/redis-stack:latest")
                    .WithName($"redis-test-harness-{Guid.NewGuid()}")
                    .WithPortBinding(hostedPort, 6379)
                    // .WithEnvironment("REDIS_ARGS", "--save '' --appendonly no")
                    // .WithTmpfsMount($"/data/{Guid.NewGuid()}") // Mount data directory in memory
                .WithWaitStrategy(
                    Wait
                        .ForUnixContainer()
                        .UntilPortIsAvailable(6379)
                        .UntilMessageIsLogged("Ready to accept connections")
                );
                ;
        }

        public async Task StartAsync(int hostedPort)
        {
            /*
                 */
            _redisContainer =
                GetRedisBuilder(hostedPort)
                    .Build();

            await
                _redisContainer
                    .StartAsync();
            /*
             */

            helper.WriteLine($"Started container: {_redisContainer.Name}");
            helper.WriteLine($"Container ID: {_redisContainer.Id}");
            helper.WriteLine($"Connection: {_redisContainer.GetConnectionString()}");
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

                /*await
                    _portNumber
                        .WaitForPortToBeReleased(_helper, TimeSpan.FromSeconds(20));*/

                /*do
                {
                    await Task.Delay(5000);
                } while (_redisContainer.State != TestcontainersStates.Undefined);*/
            }
        }
    }
}