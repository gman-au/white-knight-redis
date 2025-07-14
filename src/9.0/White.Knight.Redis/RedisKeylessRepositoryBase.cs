using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using White.Knight.Domain;
using White.Knight.Interfaces;
using White.Knight.Interfaces.Command;
using White.Knight.Redis.Options;

namespace White.Knight.Redis
{
    public abstract class RedisKeylessRepositoryBase<TD>(
        RedisRepositoryFeatures<TD> repositoryFeatures) : IKeylessRepository<TD>
        where TD : new()
    {
        private readonly IRedisCache<TD> _redisCache = repositoryFeatures.RedisCache;
        private readonly IRepositoryExceptionWrapper _repositoryExceptionWrapper = repositoryFeatures.ExceptionWrapper;
        protected readonly ILogger Logger = repositoryFeatures.LoggerFactory.CreateLogger<RedisKeylessRepositoryBase<TD>>();
        protected readonly Stopwatch Stopwatch = new();

        public abstract Expression<Func<TD, object>> DefaultOrderBy();

        public Task<RepositoryResult<TP>> QueryAsync<TP>(IQueryCommand<TD, TP> command, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }


        protected Exception RethrowRepositoryException(Exception exception)
        {
            return _repositoryExceptionWrapper != null
                ? _repositoryExceptionWrapper.Rethrow(exception)
                : exception;
        }
    }
}