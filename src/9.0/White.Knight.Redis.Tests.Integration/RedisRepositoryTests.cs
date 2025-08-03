using System.Reflection;
using System.Threading.Tasks;
using White.Knight.Redis.Injection;
using White.Knight.Redis.Tests.Integration.Repositories;
using White.Knight.Tests.Abstractions;
using White.Knight.Tests.Abstractions.Extensions;
using White.Knight.Tests.Abstractions.Repository;
using White.Knight.Tests.Abstractions.Tests;
using Xunit.Abstractions;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace White.Knight.Redis.Tests.Integration
{
    public class RedisRepositoryTests(ITestOutputHelper helper)
        : AbstractedRepositoryTests(new RedisRepositoryTestContext(helper)), IAsyncLifetime
    {
        private static readonly Assembly RepositoryAssembly =
            Assembly
                .GetAssembly(typeof(AddressRepository));

        private readonly TestContainerManager _testContainerManager = new(helper);

        public async Task InitializeAsync()
        {
            await
                _testContainerManager
                    .StartAsync();
        }

        public async Task DisposeAsync()
        {
            await
                _testContainerManager
                    .StopAsync();
        }

        private class RedisRepositoryTestContext : RepositoryTestContextBase, IRepositoryTestContext
        {
            public RedisRepositoryTestContext(
                ITestOutputHelper testOutputHelper)
            {
                // specify redis harness
                LoadTestConfiguration<RedisTestHarness>();

                // service initialisation
                ServiceCollection
                    .AddRedisRepositories(Configuration)
                    .AddAttributedRedisRepositories(RepositoryAssembly);

                // redirect ILogger output to Xunit console
                ServiceCollection
                    .ArrangeXunitOutputLogging(testOutputHelper);

                ServiceCollection
                    .AddRedisRepositoryFeatures();

                // ServiceCollection
                //     .BuildServiceProvider()
                //     .GetRequiredService<ITestHarness>();

                LoadServiceProvider();
            }
        }
    }
}