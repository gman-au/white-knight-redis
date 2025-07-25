using System;
using NRedisStack.Search;

namespace White.Knight.Redis.Extensions
{
    internal static class SchemaEx
    {
        public static void AddMappedType(this Schema schema, Type type, string fieldName)
        {
            var aliasName = $"{fieldName}";
            var jsonFieldName = $"$.{fieldName}";

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
                                .AddTextField(field);
                            return;
                        default:
                            return;
                    }
                case TypeCode.DBNull:
                    break;
                case TypeCode.Boolean:
                    schema
                        .AddTagField(field);
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
                        .AddNumericField(field);
                    return;
                case TypeCode.Char:
                case TypeCode.String:
                    schema
                        .AddTextField(field);
                    return;
                case TypeCode.DateTime:
                    schema
                        .AddTextField(field);
                        // .AddNumericField(fieldName);
                    return;
            }

            throw new NotImplementedException($"Redis mapping for type {type.FullName} not implemented");
        }
    }
}