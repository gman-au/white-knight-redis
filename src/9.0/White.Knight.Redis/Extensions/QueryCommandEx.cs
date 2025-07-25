using System;
using System.Linq.Expressions;
using White.Knight.Abstractions.Extensions;
using White.Knight.Interfaces.Command;

namespace White.Knight.Redis.Extensions
{
    internal static class QueryCommandEx
    {
        public static string ToSql<TD, TP>(
            this IQueryCommand<TD, TP> command)
        {
            var specification =
                command
                    .Specification;

            var pagingOptions =
                command
                    .PagingOptions;

            var rootQuery =
                specification
                    .ToRootQuery();

            return
                rootQuery
                    .ToIndexSearch(
                        pagingOptions?.Page,
                        pagingOptions?.PageSize,
                        GetOrderByField(pagingOptions?.OrderBy),
                        pagingOptions?.Descending
                    );
        }

        private static string GetOrderByField<TD>(Expression<Func<TD, object>> sortOrderExpression)
        {
            if (sortOrderExpression == null) return null;

            if (sortOrderExpression.Body is UnaryExpression unaryExpression) return unaryExpression.ExtractValue();

            return string.Empty;
        }
    }
}