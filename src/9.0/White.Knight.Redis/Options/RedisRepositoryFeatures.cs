using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using White.Knight.Interfaces;

namespace White.Knight.Redis.Options
{
    public class RedisRepositoryFeatures<T>(
        IRedisCache<T> redisCache,
        IRepositoryExceptionWrapper exceptionWrapper = null,
        ILoggerFactory loggerFactory = null)
        : IRepositoryOptions<T> where T : new()
    {
        public IRedisCache<T> RedisCache { get; set; } = redisCache;

        public IRepositoryExceptionWrapper ExceptionWrapper { get; set; } = exceptionWrapper;

        public ILoggerFactory LoggerFactory { get; set; } = loggerFactory ?? new NullLoggerFactory();
    }
}