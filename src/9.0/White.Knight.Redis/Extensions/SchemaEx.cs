using System;
using System.Reflection;
using NRedisStack.Search;

namespace White.Knight.Redis.Extensions
{
    internal static class SchemaEx
    {
        public static void ExpandTypeMapping(this Schema schema, Type entityType, ref int nestLevel, string prefix = null)
        {
            nestLevel++;
            if (nestLevel > 2) return;

            foreach (var propertyInfo in entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var name =
                    propertyInfo
                        .GetMemberPropertyOrJsonAlias();

                var type =
                    propertyInfo
                        .PropertyType;

                schema
                    .AddMappedType(type, name, ref nestLevel, prefix);
            }
        }

        public static void AddMappedType(this Schema schema, Type type, string fieldName, ref int nestLevel, string prefix = null)
        {
            var aliasName = $"{fieldName}";
            var jsonFieldName = $"$.{fieldName}";

            if (!string.IsNullOrEmpty(prefix))
            {
                aliasName = $"{prefix}.{aliasName}";
                jsonFieldName = $"$.{aliasName}";
            }

            var field =
                FieldName
                    .Of(jsonFieldName)
                    .As(aliasName);

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                    break;
                case TypeCode.Object:
                    switch (type.FullName)
                    {
                        case "System.Guid":
                            schema
                                .AddTagField(field, sortable: true);
                            return;
                        default:
                            schema.ExpandTypeMapping(type, ref nestLevel, fieldName);
                            return;
                    }
                case TypeCode.DBNull:
                    break;
                case TypeCode.Boolean:
                    schema
                        .AddTagField(field, sortable: true);
                    return;
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    schema
                        .AddNumericField(field, sortable: true);
                    return;
                case TypeCode.Char:
                case TypeCode.String:
                    schema
                        .AddTextField(field, sortable: true);
                    return;
                case TypeCode.DateTime:
                    schema
                        .AddTextField(field, sortable: true);
                    return;
            }

            throw new NotImplementedException($"Redis mapping for type {type.FullName} not implemented");
        }
    }
}