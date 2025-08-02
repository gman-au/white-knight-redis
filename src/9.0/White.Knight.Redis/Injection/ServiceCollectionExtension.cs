using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using White.Knight.Injection.Abstractions;
using White.Knight.Interfaces;
using White.Knight.Redis.Attribute;
using White.Knight.Redis.Options;
using White.Knight.Redis.Translator;

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

            services
                .AddScoped(typeof(ICommandTranslator<,>), typeof(RedisCommandTranslator<,>));

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

		public static IServiceCollection AddRedisRepositoryFeatures(this IServiceCollection services)
		{
			services
				.AddScoped(typeof(RedisRepositoryFeatures<>), typeof(RedisRepositoryFeatures<>))
				.AddScoped(typeof(IRedisCache<>), typeof(RedisCache<>));

			return services;
		}
	}
}