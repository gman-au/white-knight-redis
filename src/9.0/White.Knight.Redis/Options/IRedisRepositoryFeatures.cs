using White.Knight.Interfaces;

namespace White.Knight.Redis.Options
{
    public interface IRedisRepositoryFeatures<T> : IRepositoryFeatures where T : new()
    {
        public IRedisCache<T> RedisCache { get; set; }
    }
}