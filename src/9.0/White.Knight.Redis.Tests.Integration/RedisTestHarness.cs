using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using White.Knight.Redis.Extensions;
using White.Knight.Tests.Abstractions;
using White.Knight.Tests.Abstractions.Data;

namespace White.Knight.Redis.Tests.Integration
{
    public class RedisTestHarness(
        ITestDataGenerator testDataGenerator,
        IRedisMultiplexer redisMultiplexer)
        : ITestHarness
    {
        public async Task<AbstractedRepositoryTestData> SetupRepositoryTestDataAsync()
        {
            var testData =
                testDataGenerator
                    .GenerateRepositoryTestData();

            // put 'records' into tables i.e. write to redis cache in advance of the tests
            await WriteRecordsAsync(testData.Addresses, o => o.AddressId);
            await WriteRecordsAsync(testData.Customers, o => o.CustomerId);
            await WriteRecordsAsync(testData.Orders, o => o.OrderId);

            return testData;
        }

        private async Task WriteRecordsAsync<T>(IEnumerable<T> records, Expression<Func<T, object>> keyExpr) where T : new()
        {
            var connectionMultiplexer =
                await
                    redisMultiplexer
                        .GetAsync();

            var cache =
                connectionMultiplexer
                    .GetDatabase();

            var taskList = new List<Task>();

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

                var writeTask =
                    cache
                        .JSON()
                        .SetAsync(
                            new RedisKey(redisKey),
                            new RedisValue("$"),
                            new RedisValue(value)
                        );

                taskList
                    .Add(writeTask);
            }

            // Block until the write task is completed
            cache
                .WaitAll(
                    taskList
                        .ToArray()
                );

            cache
                .CreateFtIndexIfNotExists<T>();
        }
    }
}