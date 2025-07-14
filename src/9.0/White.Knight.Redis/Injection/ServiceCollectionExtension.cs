using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using White.Knight.Injection.Abstractions;
using White.Knight.Redis.Attribute;
using White.Knight.Redis.Options;

namespace White.Knight.Redis.Injection
{
	public static class ServiceCollectionExtension
	{
        public static IServiceCollection AddRedisRepositories(
            this IServiceCollection services, 
            IConfigurationRoot configuration)
        {
            services
                .Configure<RedisRepositoryConfigurationOptions>(
                    configuration
                        .GetSection(nameof(RedisRepositoryConfigurationOptions))
                );
            
            services
	            .AddSingleton<IRedisMultiplexer, RedisMultiplexer>()
                .AddTransient(typeof(IRedisCache<>), typeof(RedisCache<>));

            return services;
        }

		public static IServiceCollection AddAttributedRedisRepositories(
			this IServiceCollection services,
			Assembly repositoryAssembly
		)
		{
			services
				.AddAttributedRepositories<IsRedisRepositoryAttribute>(repositoryAssembly);
            
			return services;
		}

		public static IServiceCollection AddRedisRepositoryOptions(this IServiceCollection services)
		{
			services
				.AddScoped(typeof(RedisRepositoryFeatures<>), typeof(RedisRepositoryFeatures<>))
				.AddScoped(typeof(IRedisCache<>), typeof(RedisCache<>));

			return services;
		}
	}
}