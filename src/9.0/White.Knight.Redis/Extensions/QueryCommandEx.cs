// using System;
// using System.Linq.Expressions;
// using White.Knight.Interfaces.Command;
//
// namespace White.Knight.Redis.Extensions
// {
//     public static class QueryCommandEx
//     {
//         public static string ToSql<TD, TP>(
//             this IQueryCommand<TD, TP> command,
//             string containerName)
//         {
//             var specification =
//                 command
//                     .Specification;
//
//             var pagingOptions =
//                 command
//                     .PagingOptions;
//
//             var rootQuery =
//                 specification
//                     .ToRootQuery();
//
//             return
//                 rootQuery
//                     .ToSql(
//                         containerName,
//                         pagingOptions?.Page,
//                         pagingOptions?.PageSize,
//                         GetOrderByField(pagingOptions?.OrderBy),
//                         pagingOptions?.Descending
//                     );
//         }
//
//         private static string GetOrderByField<TD>(Expression<Func<TD, object>> sortOrderExpression)
//         {
//             if (sortOrderExpression == null) return null;
//
//             if (sortOrderExpression.Body is UnaryExpression unaryExpression)
//             {
//                 return unaryExpression.ExtractValue();
//             }
//
//             return string.Empty;
//         }
//     }
// }