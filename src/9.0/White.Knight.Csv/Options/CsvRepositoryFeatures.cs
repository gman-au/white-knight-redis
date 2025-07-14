using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using White.Knight.Interfaces;

namespace White.Knight.Csv.Options
{
    public class CsvRepositoryFeatures<T>(
        ICsvLoader<T> csvLoader,
        IRepositoryExceptionWrapper exceptionWrapper = null,
        ILoggerFactory loggerFactory = null)
        : IRepositoryOptions<T>
    {
        public ICsvLoader<T> CsvLoader { get; set; } = csvLoader;

        public IRepositoryExceptionWrapper ExceptionWrapper { get; set; } = exceptionWrapper;

        public ILoggerFactory LoggerFactory { get; set; } = loggerFactory ?? new NullLoggerFactory();
    }
}