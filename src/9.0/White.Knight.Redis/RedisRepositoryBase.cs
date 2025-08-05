using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using White.Knight.Abstractions.Extensions;
using White.Knight.Abstractions.Fluent;
using White.Knight.Interfaces;
using White.Knight.Interfaces.Command;
using White.Knight.Redis.Extensions;
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

            var redisKey =
                $"{DatabaseEx.GetKeyPrefix<TD>()}:{key}"
                    .ToLowerInvariant();

            try
            {
                Logger
                    .LogDebug("Retrieving single record with key [{redisKey}]", redisKey);

                Stopwatch
                    .Restart();

                var redisEntity =
                    await
                        _redisCache
                            .GetAsync(redisKey, cancellationToken);

                Logger
                    .LogDebug("Retrieved single record with key [{redisKey}] in {ms} ms", redisKey,
                        Stopwatch.ElapsedMilliseconds);

                return redisEntity;
            }
            catch (Exception e)
            {
                Logger
                    .LogError("Retrieving single record with key [{redisKey}]: {error}", redisKey, e.Message);

                throw RethrowRepositoryException(e);
            }
            finally
            {
                Stopwatch
                    .Stop();
            }
        }

        public virtual async Task<TD> AddOrUpdateAsync(
            IUpdateCommand<TD> command,
            CancellationToken cancellationToken = default)
        {
            return await AddOrUpdateWithModifiedAsync(
                command.Entity,
                command.Inclusions,
                command.Exclusions,
                cancellationToken
            );
        }

        public async Task<object> DeleteRecordAsync(
            ISingleRecordCommand<TD> command,
            CancellationToken cancellationToken)
        {
            var key = command.Key;

            var redisKey =
                $"{DatabaseEx.GetKeyPrefix<TD>()}:{key}"
                    .ToLowerInvariant();

            try
            {
                Logger
                    .LogDebug("Deleting record with key [{redisKey}]", redisKey);

                Stopwatch
                    .Restart();

                await
                    _redisCache
                        .RemoveAsync(redisKey, cancellationToken);

                Logger
                    .LogDebug("Deleted record with key [{redisKey}] in {ms} ms", redisKey, Stopwatch.ElapsedMilliseconds);

                return key;
            }
            catch (Exception e)
            {
                Logger
                    .LogError("Error deleting record key [{redisKey}]: {error}", redisKey, e.Message);

                throw RethrowRepositoryException(e);
            }
            finally
            {
                Stopwatch
                    .Stop();
            }
        }

        private async Task<TD> AddOrUpdateWithModifiedAsync(
            TD sourceEntity,
            Expression<Func<TD, object>>[] fieldsToModify,
            Expression<Func<TD, object>>[] fieldsToPreserve,
            CancellationToken cancellationToken
        )
        {
            TD entityToCommit;
            var redisKey = "undefined";

            try
            {
                Stopwatch
                    .Restart();

                var key =
                    KeyExpression()
                        .Compile()
                        .Invoke(sourceEntity);

                redisKey =
                    $"{DatabaseEx.GetKeyPrefix<TD>()}:{key}"
                        .ToLowerInvariant();

                Logger
                    .LogDebug("Upserting record of type [{type}] with key [{redisKey}]", DatabaseEx.GetKeyPrefix<TD>(), redisKey);

                var targetEntity =
                    await
                        _redisCache
                            .GetAsync(redisKey, cancellationToken);

                entityToCommit =
                    sourceEntity
                        .ApplyInclusionStrategy(
                            targetEntity,
                            fieldsToModify,
                            fieldsToPreserve);

                await
                    _redisCache
                        .SetAsync(redisKey, entityToCommit, cancellationToken);

                Logger
                    .LogDebug("Upserted record of type [{type}] with key [{redisKey}] in {ms} ms", DatabaseEx.GetKeyPrefix<TD>(), redisKey,
                        Stopwatch.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Logger
                    .LogError("Error upserting record of type [{type}] with key [{redisKey}]: {error}", DatabaseEx.GetKeyPrefix<TD>(), redisKey, e.Message);

                throw RethrowRepositoryException(e);
            }
            finally
            {
                Stopwatch
                    .Stop();
            }

            return entityToCommit;
        }
    }
}