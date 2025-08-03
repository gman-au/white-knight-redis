using System.Threading.Tasks;
using StackExchange.Redis;

namespace White.Knight.Redis.Tests.Integration.Extensions
{
    public static class RedisMultiplexerEx
    {
        public static async Task FlushCacheAsync(this IConnectionMultiplexer connectionMultiplexer)
        {
            foreach (var endpoint in connectionMultiplexer.GetEndPoints())
            {
                var server =
                    connectionMultiplexer
                        .GetServer(endpoint);

                await
                    server
                        .FlushAllDatabasesAsync();
            }
        }
    }
}