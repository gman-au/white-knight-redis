using White.Knight.Interfaces;
using White.Knight.Redis.Translator;

namespace White.Knight.Redis.Options
{
    public interface IRedisRepositoryFeatures<T> : IRepositoryFeatures where T : new()
    {
        public IRedisCache<T> RedisCache { get; set; }

        public ICommandTranslator<T, RedisTranslationResult> CommandTranslator { get; set; }
    }
}