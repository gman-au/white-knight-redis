using System.Threading;
using System.Threading.Tasks;

namespace White.Knight.Redis
{
    public interface IRedisCache<T> where T : new()
    {
        Task<T> GetAsync(object key, CancellationToken cancellationToken);
    }
}