using White.Knight.Abstractions.Options;

namespace White.Knight.Redis.Options
{
    public class RedisRepositoryConfigurationOptions : RepositoryConfigurationOptions

    {
        public string ConnectionString { get; set; }
    }
}