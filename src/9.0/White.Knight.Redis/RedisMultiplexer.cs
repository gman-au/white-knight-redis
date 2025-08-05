using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using White.Knight.Redis.Options;

namespace White.Knight.Redis
{
    public class RedisMultiplexer : IRedisMultiplexer
    {
        private readonly ILogger<RedisMultiplexer> _logger;
        private readonly RedisRepositoryConfigurationOptions _options;
        private bool _connected;
        private IConnectionMultiplexer _connectionMultiplexer;

        public RedisMultiplexer(
            IOptions<RedisRepositoryConfigurationOptions> optionsAccessor,
            ILoggerFactory loggerFactory = null)
        {
            _logger =
                (loggerFactory ??
                 NullLoggerFactory.Instance)
                .CreateLogger<RedisMultiplexer>();

            _options =
                optionsAccessor
                    .Value;
        }

        public async Task<IConnectionMultiplexer> GetAsync()
        {
            try
            {
                // if (_connected)
                //     return _connectionMultiplexer;

                _connectionMultiplexer =
                    await
                        ConnectionMultiplexer
                            .ConnectAsync($"{_options.ConnectionString},allowAdmin=true");

                _connected = true;

                return _connectionMultiplexer;
            }
            catch (RedisConnectionException ex)
            {
                _logger
                    .LogError("Error connecting to Redis client [{client}]: {error}", _connectionMultiplexer?.ClientName,
                        ex.Message);

                throw;
            }
        }
    }
}