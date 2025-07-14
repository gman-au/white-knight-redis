using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using White.Knight.Abstractions.Extensions;
using White.Knight.Abstractions.Fluent;
using White.Knight.Interfaces;
using White.Knight.Interfaces.Command;
using White.Knight.Redis.Options;

namespace White.Knight.Redis
{
    public abstract class RedisRepositoryBase<TD>(
        RedisRepositoryFeatures<TD> repositoryFeatures)
        : RedisKeylessRepositoryBase<TD>(repositoryFeatures), IRepository<TD>
        where TD : new()
    {
        private readonly IRedisCache<TD> _redisCache = repositoryFeatures.RedisCache;

        public override Expression<Func<TD, object>> DefaultOrderBy()
        {
            return KeyExpression();
        }

        public abstract Expression<Func<TD, object>> KeyExpression();

        public virtual async Task<TD> SingleRecordAsync(object key, CancellationToken cancellationToken)
        {
            return await
                SingleRecordAsync(
                    key
                        .ToSingleRecordCommand<TD>(),
                    cancellationToken
                );
        }

        public async Task<TD> SingleRecordAsync(ISingleRecordCommand<TD> command, CancellationToken cancellationToken)
        {
            var key = command.Key;

            try
            {
                Logger
                    .LogDebug("Retrieving single record with key [{key}]", key);

                Stopwatch
                    .Restart();

                var selector =
                    key
                        .BuildKeySelectorExpression(KeyExpression());

                var csvEntity =
                    await
                        _redisCache
                            .GetAsync(key, cancellationToken);

                Logger
                    .LogDebug("Retrieved single record with key [{key}] in {ms} ms", key,
                        Stopwatch.ElapsedMilliseconds);

                return csvEntity;
            }
            catch (Exception e)
            {
                Logger
                    .LogError("Retrieving single record with key [{key}]: {error}", key, e.Message);

                throw RethrowRepositoryException(e);
            }
            finally
            {
                Stopwatch
                    .Stop();
            }
        }

        public Task<TD> AddOrUpdateAsync(IUpdateCommand<TD> command, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<object> DeleteRecordAsync(ISingleRecordCommand<TD> command, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }
    }
}