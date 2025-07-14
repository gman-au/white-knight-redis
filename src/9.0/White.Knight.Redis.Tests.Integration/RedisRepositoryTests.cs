using System.Reflection;
using White.Knight.Redis.Injection;
using White.Knight.Redis.Tests.Integration.Repositories;
using White.Knight.Tests.Abstractions;
using White.Knight.Tests.Abstractions.Extensions;
using White.Knight.Tests.Abstractions.Repository;
using White.Knight.Tests.Abstractions.Tests;
using Xunit.Abstractions;

namespace White.Knight.Redis.Tests.Integration
{
    public class RedisRepositoryTests(ITestOutputHelper helper)
        : AbstractedRepositoryTests(new CsvRepositoryTestContext(helper))
    {
        private static readonly Assembly RepositoryAssembly =
            Assembly
                .GetAssembly(typeof(AddressRepository));

        private class CsvRepositoryTestContext : RepositoryTestContextBase, IRepositoryTestContext
        {
            public CsvRepositoryTestContext(ITestOutputHelper testOutputHelper)
            {
                // specify csv harness
                LoadTestConfiguration<RedisTestHarness>();

                // service initialisation
                ServiceCollection
                    .AddRedisRepositories(Configuration)
                    .AddAttributedRedisRepositories(RepositoryAssembly);

                // redirect ILogger output to Xunit console
                ServiceCollection
                    .ArrangeXunitOutputLogging(testOutputHelper);

                ServiceCollection
                    .AddRedisRepositoryOptions();

                LoadServiceProvider();
            }
        }
    }
}