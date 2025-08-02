using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using White.Knight.Interfaces;
using White.Knight.Redis.Translator;

namespace White.Knight.Redis.Options
{
    public class RedisRepositoryFeatures<TD>(
        IRedisCache<TD> redisCache,
        ICommandTranslator<TD, RedisTranslationResult> commandTranslator,
        IRepositoryExceptionRethrower exceptionRethrower = null,
        ILoggerFactory loggerFactory = null)
        : IRedisRepositoryFeatures<TD> where TD : new()
    {
        public IRedisCache<TD> RedisCache { get; set; } = redisCache;

        public ICommandTranslator<TD, RedisTranslationResult> CommandTranslator { get; set; } = commandTranslator;

        public IRepositoryExceptionRethrower ExceptionRethrower { get; set; } = exceptionRethrower;

        public ILoggerFactory LoggerFactory { get; set; } = loggerFactory ?? new NullLoggerFactory();
    }
}