using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using White.Knight.Csv.Injection;
using White.Knight.Csv.Options;
using White.Knight.Csv.Tests.Integration.Repositories;
using White.Knight.Tests.Abstractions;
using White.Knight.Tests.Abstractions.Injection;
using White.Knight.Tests.Abstractions.Tests;
using White.Knight.Tests.Domain;

namespace White.Knight.Csv.Tests.Integration
{
    public class CsvInjectionTests() : AbstractedInjectionTests(new CsvInjectionTestContext())
    {
        private static readonly Assembly RepositoryAssembly =
            Assembly
                .GetAssembly(typeof(AddressRepository));

        private class CsvInjectionTestContext : InjectionTestContextBase, IInjectionTestContext
        {
            public override void ArrangeImplementedServices()
            {
                ServiceCollection
                    .AddCsvRepositories(Configuration)
                    .AddAttributedCsvRepositories(RepositoryAssembly);

                ServiceCollection
                    .AddCsvRepositoryOptions();
            }

            public override void AssertLoggerFactoryResolved()
            {
                var features =
                    Sut
                        .GetRequiredService<CsvRepositoryFeatures<Address>>();

                Assert
                    .NotNull(features);

                var loggerFactory =
                    features
                        .LoggerFactory;

                Assert
                    .NotNull(loggerFactory);
            }
        }
    }
}