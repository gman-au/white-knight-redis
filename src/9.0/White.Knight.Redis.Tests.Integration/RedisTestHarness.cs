using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using White.Knight.Tests.Abstractions;
using White.Knight.Tests.Abstractions.Data;

namespace White.Knight.Redis.Tests.Integration
{
    public class RedisTestHarness : ITestHarness
    {
        private readonly IRedisMultiplexer _redisMultiplexer;
        private readonly ITestDataGenerator _testDataGenerator;

        public RedisTestHarness(
            ITestDataGenerator testDataGenerator,
            IRedisMultiplexer redisMultiplexer)
        {
            _testDataGenerator = testDataGenerator;
            _redisMultiplexer = redisMultiplexer;

            try
            {
                /*var redisContainer =
                    new RedisBuilder()
                        .WithImage("redis/redis-stack:latest")
                        .WithName("redis-test-harness")
                        .WithPortBinding(6379, 6379)
                        .Build();

                redisContainer
                    .StartAsync()
                    .Wait();*/
            }
            catch (Exception ex)
            {
                Assert
                    .Fail($"Could not set up Redis container: {ex.Message}");
            }
        }

        public async Task<AbstractedRepositoryTestData> GenerateRepositoryTestDataAsync()
        {
            var testData =
                _testDataGenerator
                    .GenerateRepositoryTestData();

            // put 'records' into tables i.e. write to CSV files in advance of the tests
            await WriteRecordsAsync(testData.Addresses, o => o.AddressId);
            await WriteRecordsAsync(testData.Customers, o => o.CustomerId);
            await WriteRecordsAsync(testData.Orders, o => o.OrderId);

            return testData;
        }

        private async Task WriteRecordsAsync<T>(IEnumerable<T> records, Expression<Func<T, object>> keyExpr)
        {
            var connectionMultiplexer =
                await
                    _redisMultiplexer
                        .GetAsync();

            var cache =
                connectionMultiplexer
                    .GetDatabase();

            foreach (var record in records)
            {
                var key =
                    keyExpr
                        .Compile()
                        .Invoke(record);

                var redisKey =
                    $"{typeof(T).Name}:{key}"
                        .ToLowerInvariant();

                var value =
                    JsonSerializer
                        .Serialize(record);

                await
                    cache
                        .JSON()
                        .SetAsync(
                            new RedisKey(redisKey),
                            new RedisValue("$"),
                            new RedisValue(value)
                        );
            }
        }
    }
}