using System.Threading.Tasks;
using StackExchange.Redis;

namespace White.Knight.Redis
{
    public interface IRedisMultiplexer
    {
        Task<IConnectionMultiplexer> GetAsync();
    }
}