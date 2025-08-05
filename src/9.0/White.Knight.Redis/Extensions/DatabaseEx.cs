using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using NRedisStack.RedisStackCommands;
using NRedisStack.Search;
using NRedisStack.Search.DataTypes;
using NRedisStack.Search.Literals.Enums;
using StackExchange.Redis;

namespace White.Knight.Redis.Extensions
{
    public static class DatabaseEx
    {
        public static string GetIndexName<TD>() where TD : new()
        {
            return $"{GetKeyPrefix<TD>()}Idx";
        }

        public static string GetKeyPrefix<TD>() where TD : new()
        {
            var entityType = typeof(TD);

            var entityTypeName =
                entityType
                    .Name
                    .ToLower();

            return entityTypeName;
        }

        public static void CreateFtIndexIfNotExists<TD>(
            this IDatabase cache,
            ILogger logger = null
        ) where TD : new()
        {
            var entityType = typeof(TD);

            var entityTypeName =
                entityType
                    .Name
                    .ToLower();

            var indexStringBuilder = new StringBuilder();

            var indexName = GetIndexName<TD>();

            indexStringBuilder
                .Append($"FT.CREATE {entityTypeName}Idx ON JSON PREFIX 1 {entityTypeName}: SCHEMA ");

            var schema = new Schema();

            foreach (var propertyInfo in entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var name =
                    propertyInfo
                        .Name;

                var type =
                    propertyInfo
                        .PropertyType;

                schema
                    .AddMappedType(type, name);
            }

            InfoResult infoResult = null;

            try
            {
                infoResult =
                    cache
                        .FT()
                        .Info(new RedisValue(indexName));

                logger?
                    .LogInformation("Created index {indexName}", indexName);
            }
            catch (RedisServerException ex)
            {
                if (
                    !$"{indexName}: no such index".Equals(ex.Message) &&
                    !"Unknown index name".Equals(ex.Message)
                )
                    throw;
            }

            logger?
                .LogWarning("Index {indexName} already exists", indexName);

            if (infoResult != null)
                cache
                    .FT()
                    .DropIndex(indexName);

            var indexTask =
                cache
                    .FT()
                    .CreateAsync(
                        indexName,
                        new FTCreateParams()
                            .On(IndexDataType.JSON)
                            .Prefix($"{entityTypeName}:"),
                        schema
                    );

            cache
                .Wait(indexTask);
        }
    }
}