using System.Threading;
using System.Threading.Tasks;

namespace White.Knight.Redis
{
    public interface IRedisCache<T> where T : new()
    {
        Task CreateOrUpdateIndexAsync();

        Task<T> GetAsync(object key, CancellationToken cancellationToken);

        Task SetAsync(object key, T value, CancellationToken cancellationToken);

        Task<bool> RemoveAsync(object key, CancellationToken cancellationToken);
    }
}