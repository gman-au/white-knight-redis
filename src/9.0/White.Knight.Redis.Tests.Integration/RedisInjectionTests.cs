using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using White.Knight.Domain.Enum;
using White.Knight.Injection.Abstractions;
using White.Knight.Redis.Injection;
using White.Knight.Redis.Options;
using White.Knight.Redis.Tests.Integration.Repositories;
using White.Knight.Tests.Abstractions;
using White.Knight.Tests.Abstractions.Extensions;
using White.Knight.Tests.Abstractions.Injection;
using White.Knight.Tests.Abstractions.Tests;
using White.Knight.Tests.Domain;

namespace White.Knight.Redis.Tests.Integration
{
    public class RedisInjectionTests() : AbstractedInjectionTests(new RedisInjectionTestContext())
    {
        private static readonly Assembly RepositoryAssembly =
            Assembly
                .GetAssembly(typeof(AddressRepository));

        private class RedisInjectionTestContext : InjectionTestContextBase, IInjectionTestContext
        {
            public override void ArrangeImplementedServices()
            {
                ServiceCollection
                    .AddRedisRepositories(Configuration)
                    .AddAttributedRedisRepositories(RepositoryAssembly);

                ServiceCollection
                    .AddRepositoryFeatures<RedisRepositoryConfigurationOptions>(Configuration)
                    .AddRedisRepositoryFeatures();
            }

            public override void ArrangeDefinedClientSideConfiguration()
            {
                Configuration =
                    Configuration
                        .ArrangeThrowOnClientSideEvaluation<RedisRepositoryConfigurationOptions>();
            }

            public override void AssertLoggerFactoryResolved()
            {
                var features =
                    Sut
                        .GetRequiredService<RedisRepositoryFeatures<Address>>();

                Assert
                    .NotNull(features);

                var loggerFactory =
                    features
                        .LoggerFactory;

                Assert
                    .NotNull(loggerFactory);
            }

            public override void AssertRepositoryOptionsResolvedWithDefault()
            {
                var options =
                    Sut
                        .GetRequiredService<IOptions<RedisRepositoryConfigurationOptions>>();

                Assert.NotNull(options.Value);

                Assert.Equal(ClientSideEvaluationResponseTypeEnum.Warn, options.Value.ClientSideEvaluationResponse);
            }

            public override void AssertRepositoryOptionsResolvedWithDefined()
            {
                var options =
                    Sut
                        .GetRequiredService<IOptions<RedisRepositoryConfigurationOptions>>();

                Assert.NotNull(options.Value);

                Assert.Equal(ClientSideEvaluationResponseTypeEnum.Throw, options.Value.ClientSideEvaluationResponse);
            }
        }
    }
}