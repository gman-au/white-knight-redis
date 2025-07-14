using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using White.Knight.Redis.Options;

namespace White.Knight.Redis
{
    public class RedisCache<TD>(
        IRedisMultiplexer redisMultiplexer,
        IOptions<RedisRepositoryConfigurationOptions> optionsAccessor)
        : IRedisCache<TD>
        where TD : new()
    {
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
                        .StringGetAsync(key.ToString());

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
    }
}