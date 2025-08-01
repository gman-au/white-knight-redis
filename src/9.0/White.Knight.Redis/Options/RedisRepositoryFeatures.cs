using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using White.Knight.Interfaces;

namespace White.Knight.Redis.Options
{
    public class RedisRepositoryFeatures<T>(
        IRedisCache<T> redisCache,
        IRepositoryExceptionRethrower exceptionRethrower = null,
        ILoggerFactory loggerFactory = null)
        : IRedisRepositoryFeatures<T> where T : new()
    {
        public IRedisCache<T> RedisCache { get; set; } = redisCache;

        public IRepositoryExceptionRethrower ExceptionRethrower { get; set; } = exceptionRethrower;

        public ILoggerFactory LoggerFactory { get; set; } = loggerFactory ?? new NullLoggerFactory();
    }
}