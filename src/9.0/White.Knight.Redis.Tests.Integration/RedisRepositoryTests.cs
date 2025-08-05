using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
        : AbstractedRepositoryTests(new RedisRepositoryTestContext(helper)), IAsyncLifetime
    {
        private static readonly Assembly RepositoryAssembly =
            Assembly
                .GetAssembly(typeof(AddressRepository));

        private readonly TestContainerManager _testContainerManager = new(helper);

        public async Task InitializeAsync()
        {
            var context = GetContext() as RedisRepositoryTestContext;

            await
                _testContainerManager
                    .StartAsync(context.GetHostedPort());
        }

        public async Task DisposeAsync()
        {
            await
                _testContainerManager
                    .StopAsync();
        }

        private class RedisRepositoryTestContext : RepositoryTestContextBase, IRepositoryTestContext
        {
            private readonly int _hostedPort;

            public RedisRepositoryTestContext(ITestOutputHelper testOutputHelper)
            {
                _hostedPort =
                    new Random()
                        .Next(10000, 11000);

                // specify redis harness
                LoadTestConfiguration<RedisTestHarness>();

                Configuration =
                    InterceptConfiguration(Configuration, _hostedPort);

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

            public int GetHostedPort()
            {
                return _hostedPort;
            }

            private static IConfigurationRoot InterceptConfiguration(IConfigurationRoot existingConfiguration, int hostedPort)
            {
                var inMemoryCollection = new Dictionary<string, string>
                {
                    ["RedisRepositoryConfigurationOptions:ConnectionString"] = $"localhost:{hostedPort}"
                };

                // Add the in-memory collection to the configuration
                return new ConfigurationBuilder()
                    .AddConfiguration(existingConfiguration)
                    .AddInMemoryCollection(inMemoryCollection)
                    .Build();
            }
        }
    }
}