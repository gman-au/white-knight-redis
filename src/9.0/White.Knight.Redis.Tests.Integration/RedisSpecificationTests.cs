using System.Reflection;
using White.Knight.Redis.Injection;
using White.Knight.Redis.Tests.Integration.Repositories;
using White.Knight.Redis.Translator;
using White.Knight.Tests.Abstractions;
using White.Knight.Tests.Abstractions.Extensions;
using White.Knight.Tests.Abstractions.Spec;
using White.Knight.Tests.Abstractions.Tests;
using White.Knight.Tests.Domain.Specifications;
using Xunit.Abstractions;

namespace White.Knight.Redis.Tests.Integration
{
    public class RedisSpecificationTests(ITestOutputHelper helper)
        : AbstractedSpecificationTests(new InMemorySpecificationTestContext(helper))
    {
        private static readonly Assembly SpecAssembly =
            Assembly
                .GetAssembly(typeof(CustomerSpecByCustomerName));

        private static readonly Assembly RepositoryAssembly =
            Assembly
                .GetAssembly(typeof(AddressRepository));

        private class InMemorySpecificationTestContext : SpecificationTestContextBase<RedisTranslationResult>, ISpecificationTestContext
        {
            public InMemorySpecificationTestContext(ITestOutputHelper testOutputHelper)
            {
                SpecificationAssembly = SpecAssembly;

                // specify in memory harness
                LoadTestConfiguration();

                // service initialisation
                ServiceCollection
                    .AddRedisRepositories(Configuration)
                    .AddAttributedRedisRepositories(RepositoryAssembly);

                // redirect ILogger output to Xunit console
                ServiceCollection
                    .ArrangeXunitOutputLogging(testOutputHelper);

                ServiceCollection
                    .AddRedisRepositoryFeatures();

                LoadServiceProvider();
            }
        }
    }
}