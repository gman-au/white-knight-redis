using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using White.Knight.Domain;
using White.Knight.Interfaces;
using White.Knight.Interfaces.Command;
using White.Knight.Redis.Extensions;
using White.Knight.Redis.Options;

namespace White.Knight.Redis
{
    public abstract class RedisKeylessRepositoryBase<TD> : IKeylessRepository<TD>
        where TD : new()
    {
        private readonly IRedisCache<TD> _redisCache;
        private readonly IRepositoryExceptionWrapper _repositoryExceptionWrapper;
        protected readonly ILogger Logger;
        protected readonly Stopwatch Stopwatch = new();

        protected RedisKeylessRepositoryBase(RedisRepositoryFeatures<TD> repositoryFeatures)
        {
            _redisCache = repositoryFeatures.RedisCache;
            _repositoryExceptionWrapper = repositoryFeatures.ExceptionWrapper;

            Logger =
                repositoryFeatures
                    .LoggerFactory
                    .CreateLogger<RedisKeylessRepositoryBase<TD>>();

            _redisCache
                .CreateOrUpdateIndexAsync()
                .Wait();
        }

        public abstract Expression<Func<TD, object>> DefaultOrderBy();

        public Task<RepositoryResult<TP>> QueryAsync<TP>(IQueryCommand<TD, TP> command, CancellationToken cancellationToken = new CancellationToken())
        {
            var sqlQueryText =
                command
                    .ToSql();

            Logger?
                .LogDebug(
                    $"SQL Query: [{sqlQueryText}]"
                );

            return null;
        }


        protected Exception RethrowRepositoryException(Exception exception)
        {
            return _repositoryExceptionWrapper != null
                ? _repositoryExceptionWrapper.Rethrow(exception)
                : exception;
        }
    }
}