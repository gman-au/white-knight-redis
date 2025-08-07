using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using White.Knight.Abstractions.Extensions;
using White.Knight.Domain;
using White.Knight.Domain.Exceptions;
using White.Knight.Interfaces;
using White.Knight.Interfaces.Command;
using White.Knight.Redis.Options;
using White.Knight.Redis.Translator;

namespace White.Knight.Redis
{
    public abstract class RedisKeylessRepositoryBase<TD> : IKeylessRepository<TD>
        where TD : new()
    {
        private readonly ICommandTranslator<TD, RedisTranslationResult> _commandTranslator;
        private readonly IClientSideEvaluationHandler _clientSideEvaluationHandler;
        private readonly IRedisCache<TD> _redisCache;
        private readonly IRepositoryExceptionRethrower _repositoryExceptionWrapper;
        protected readonly ILogger Logger;
        protected readonly Stopwatch Stopwatch = new();

        protected RedisKeylessRepositoryBase(RedisRepositoryFeatures<TD> repositoryFeatures)
        {
            _redisCache = repositoryFeatures.RedisCache;
            _repositoryExceptionWrapper = repositoryFeatures.ExceptionRethrower;
            _commandTranslator = repositoryFeatures.CommandTranslator;
            _clientSideEvaluationHandler = repositoryFeatures.ClientSideEvaluationHandler;

            Logger =
                repositoryFeatures
                    .LoggerFactory
                    .CreateLogger<RedisKeylessRepositoryBase<TD>>();
        }

        public abstract Expression<Func<TD, object>> DefaultOrderBy();

        public async Task<RepositoryResult<TP>> QueryAsync<TP>(
            IQueryCommand<TD, TP> command,
            CancellationToken cancellationToken = new()
        )
        {
            // create index if it does not exist
            await
                _redisCache
                    .CreateOrUpdateIndexAsync();

            try
            {
                var translationResult =
                    _commandTranslator
                        .Translate(command);

                if (translationResult == null)
                    throw new Exception("There was an error translating the Redis command.");

                Logger?
                    .LogDebug(
                        "Redis Query: [{query}], Sort: [{sortField}, desc={desc}]",
                        translationResult.Query,
                        translationResult.SortByField ?? "<undefined>",
                        translationResult.SortDescending?.ToString() ?? "<undefined>"
                    );

                var result =
                    await
                        _redisCache
                            .QueryAsync(
                                translationResult.Query,
                                translationResult.SortByField,
                                translationResult.SortDescending,
                                translationResult.Page,
                                translationResult.PageSize,
                                cancellationToken
                            );

                var queryable = result.Item1;
                var count = result.Item2;

                return new RepositoryResult<TP>
                {
                    Records =
                        queryable
                            .Select(o =>
                                command
                                    .ProjectionOptions
                                    .Projection
                                    .Compile()
                                    .Invoke(o)
                            ),
                    Count = count
                };
            }
            catch (UnparsableSpecificationException)
            {
                _clientSideEvaluationHandler
                    .Handle<TD>();

                var queryable =
                    await
                        _redisCache
                            .GetAllAsync(cancellationToken);

                return
                    await
                        queryable
                            .ApplyCommandQueryAsync(command);
            }
        }

        protected Exception RethrowRepositoryException(Exception exception)
        {
            return _repositoryExceptionWrapper != null
                ? _repositoryExceptionWrapper.Rethrow(exception)
                : exception;
        }
    }
}