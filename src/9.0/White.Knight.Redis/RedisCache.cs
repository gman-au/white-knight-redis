using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using StackExchange.Redis;
using White.Knight.Redis.Extensions;
using White.Knight.Redis.Options;

namespace White.Knight.Redis
{
    public class RedisCache<TD>(
        IRedisMultiplexer redisMultiplexer,
        IOptions<RedisRepositoryConfigurationOptions> optionsAccessor,
        ILoggerFactory loggerFactory = null)
        : IRedisCache<TD>
        where TD : new()
    {
        private readonly ILogger<RedisMultiplexer> _logger =
            (loggerFactory ?? NullLoggerFactory.Instance)
            .CreateLogger<RedisMultiplexer>();

        public async Task CreateOrUpdateIndexAsync()
        {
            var connectionMultiplexer =
                await
                    redisMultiplexer
                        .GetAsync();

            var cache =
                connectionMultiplexer
                    .GetDatabase();

            cache
                .CreateFtIndexIfNotExists<TD>();
        }

        public async Task<TD> GetAsync(object key, CancellationToken cancellationToken)
        {
            var connectionMultiplexer =
                await
                    redisMultiplexer
                        .GetAsync();

            var cache =
                connectionMultiplexer
                    .GetDatabase();

            var result =
                await
                    cache
                        .JSON()
                        .GetAsync(
                            key.ToString()
                        );

            if (result.IsNull)
                return default;

            var bytes =
                Encoding
                    .UTF8
                    .GetBytes(result.ToString());

            var value =
                JsonSerializer
                    .Deserialize<TD>(bytes);

            return value;
        }

        public async Task<IQueryable<TD>> GetAllAsync(CancellationToken cancellationToken)
        {
            var connectionMultiplexer =
                await
                    redisMultiplexer
                        .GetAsync();

            var cache =
                connectionMultiplexer
                    .GetDatabase();

            var results = new List<TD>();

            var keyPattern = $"{DatabaseEx.GetKeyPrefix<TD>()}:*";

            foreach (var endpoint in connectionMultiplexer.GetEndPoints())
            {
                var server =
                    connectionMultiplexer
                        .GetServer(endpoint);

                await foreach (var key in server.KeysAsync(pattern: keyPattern).WithCancellation(cancellationToken))
                {
                    var cacheValue =
                        await
                            cache
                                .JSON()
                                .GetAsync<TD>(key);

                    if (cacheValue != null)
                        results.Add(cacheValue);
                }
            }

            return
                results
                    .AsQueryable();
        }

        public async Task SetAsync(object key, TD value, CancellationToken cancellationToken)
        {
            var connectionMultiplexer =
                await
                    redisMultiplexer
                        .GetAsync();

            var cache =
                connectionMultiplexer
                    .GetDatabase();

            var cacheValue =
                JsonSerializer
                    .Serialize(value);

            await
                cache
                    .JSON()
                    .SetAsync(
                        new RedisKey(key.ToString()),
                        new RedisValue("$"),
                        cacheValue
                    );

            // TODO: caching expiry can / should be a redis config option
        }

        public async Task<bool> RemoveAsync(object key, CancellationToken cancellationToken)
        {
            var connectionMultiplexer =
                await
                    redisMultiplexer
                        .GetAsync();

            var cache =
                connectionMultiplexer
                    .GetDatabase();
            try
            {
                var deletedKeys = 0;

                await foreach (var keyMatch in GetKeysAsync(connectionMultiplexer, key.ToString())
                                   .WithCancellation(cancellationToken))
                {
                    var result =
                        await
                            cache
                                .KeyDeleteAsync(keyMatch);

                    if (result)
                    {
                        _logger
                            .LogDebug("Redis successfully removed cached entry at [{keyMatch}]", keyMatch);
                        deletedKeys++;
                    }
                    else
                    {
                        _logger
                            .LogWarning("Redis did not remove cached entry at [{keyMatch}]", keyMatch);
                    }
                }

                return deletedKeys > 0;
            }
            catch (RedisException ex)
            {
                _logger
                    .LogWarning("Error removing cached entry at [{keyMatch}]", ex.Message);

                return false;
            }
        }

        public async Task<(IQueryable<TD>, long)> QueryAsync(string queryString, CancellationToken cancellationToken)
        {
            var connectionMultiplexer =
                await
                    redisMultiplexer
                        .GetAsync();

            var cache =
                connectionMultiplexer
                    .GetDatabase();

            var result =
                await
                    cache
                        .FT()
                        .SearchAsync(
                            DatabaseEx.GetIndexName<TD>(),
                            new Query(queryString));

            return
            (
                result
                    .ToJson()
                    .Select(o => JsonSerializer.Deserialize<TD>(o))
                    .AsQueryable(),
                result
                    .Documents
                    .Count
            );
        }

        private static async IAsyncEnumerable<string> GetKeysAsync(IConnectionMultiplexer connectionMultiplexer,
            string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(pattern));

            foreach (var endpoint in connectionMultiplexer.GetEndPoints())
            {
                var server =
                    connectionMultiplexer
                        .GetServer(endpoint);

                foreach (var key in server.Keys(pattern: pattern))
                    yield return key.ToString();
            }
        }
    }
}