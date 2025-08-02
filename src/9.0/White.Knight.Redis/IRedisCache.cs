using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace White.Knight.Redis
{
    public interface IRedisCache<T> where T : new()
    {
        Task CreateOrUpdateIndexAsync();

        Task<T> GetAsync(object key, CancellationToken cancellationToken);

        Task<IQueryable<T>> GetAllAsync(CancellationToken cancellationToken);

        Task SetAsync(object key, T value, CancellationToken cancellationToken);

        Task<bool> RemoveAsync(object key, CancellationToken cancellationToken);

        Task<(IQueryable<T>, long)> QueryAsync(
            string queryString,
            string sortByField,
            bool? sortDescending,
            int? page,
            int? pageSize,
            CancellationToken cancellationToken = default);
    }
}