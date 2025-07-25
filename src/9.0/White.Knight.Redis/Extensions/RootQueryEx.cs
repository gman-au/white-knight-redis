using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using White.Knight.Abstractions.Definition;
using White.Knight.Domain.Exceptions;

namespace White.Knight.Redis.Extensions
{
    internal static class RootQueryEx
    {
        private static readonly Action<StringBuilder, string> BuildAction = (b, s) => b.Append(s);

        public static string ToIndexSearch(
            this RootQuery rootQuery,
            int? page,
            int? pageSize,
            string orderBy,
            bool? descending
        )
        {
            if (rootQuery == null)
                throw new UnparsableSpecificationException();

            var result = new StringBuilder();

            var alias =
                rootQuery
                    .Alias;

            BuildAction(
                result,
                $"FT.SEARCH idx:{alias}"
            );

            ParseSelfJoins(
                rootQuery.Joins,
                result,
                alias
            );
            
            BuildAction(result, " WHERE");

            var subQuery = rootQuery.Query;
            
            ParseSubQuery(
                subQuery,
                result,
                alias,
                rootQuery
            );

            ParseOrderBy(
                result,
                orderBy,
                descending,
                alias
            );

            ParsePaging(
                result,
                page,
                pageSize
            );

            result.Replace("   ","  ");
            result.Replace("  "," ");

            return
                result
                    .ToString();
        }

        private static void ParseSelfJoins(IEnumerable<SelfJoin> selfJoins, StringBuilder result, string alias)
        {
            if (selfJoins == null) return;
            
            foreach (var selfJoin in selfJoins)
            {
                BuildAction(
                    result,
                    $" JOIN {selfJoin.Alias} IN {alias}.{selfJoin.ChildSet}"
                );
            }
        }

        private static void ParsePaging(StringBuilder result, int? page, int? pageSize)
        {
            if (page.HasValue)
            {
                var skip = pageSize * (page - 1);

                if (skip.HasValue)
                {
                    BuildAction(
                        result,
                        $" OFFSET {skip}"
                    );
                }
            }

            if (pageSize.HasValue)
            {
                BuildAction(
                    result,
                    $" LIMIT {pageSize}"
                );
            }
        }

        private static void ParseOrderBy(StringBuilder result, string orderBy, bool? descending, string alias)
        {
            if (!string.IsNullOrEmpty(orderBy))
            {
                BuildAction(
                    result,
                    $" ORDER BY {alias}.{orderBy} {(descending == true ? "DESC" : null)}"
                );
            }
        }

        private static void ParseSubQuery(this ISubQuery subQuery, StringBuilder stringBuilder, string alias, RootQuery rootQuery)
        {
            var relatedJoin =
                (rootQuery.Joins ?? Enumerable.Empty<SelfJoin>())
                .FirstOrDefault(o => o.SubQuery == subQuery);

            if (relatedJoin != null)
            {
                alias = relatedJoin.Alias;
            }
            
            if (subQuery is StringSubQuery stringSubQuery)
            {
                BuildAction(
                    stringBuilder,
                    $" {stringSubQuery.Operator.MapOperator()($"{alias}.{stringSubQuery.OperandLeft}", stringSubQuery.OperandRight.Map(stringSubQuery.OperandType))}"
                );
            }
            else
            {
                if (subQuery is QuerySubQuery querySubQuery)
                {
                    var leftQueryBuilder = new StringBuilder();
                    if (querySubQuery.OperandLeft is ISubQuery leftSubQuery)
                    {
                        leftSubQuery
                            .ParseSubQuery(
                                leftQueryBuilder,
                                alias,
                                rootQuery
                            );
                    }

                    var rightQueryBuilder = new StringBuilder();
                    if (querySubQuery.OperandRight is ISubQuery rightSubQuery)
                    {
                        rightSubQuery
                            .ParseSubQuery(
                                rightQueryBuilder,
                                alias,
                                rootQuery
                            );
                    }

                    BuildAction(
                        stringBuilder,
                        $" {querySubQuery.Operator.MapOperator()(leftQueryBuilder.ToString(), rightQueryBuilder.ToString())}"
                    );
                }
            }
        }

        private static string Map(this object value, Type knownType)
        {
            if (knownType == typeof(string)) return $"\"{value}\"";
            if (knownType == typeof(Guid)) return $"\"{value}\"";
            if (knownType == typeof(DateTime)) return $"\"{value}\"";

            return value.ToString();
        }

        private static Func<string, string, string> MapOperator(this string expressionType)
        {
            switch (expressionType)
            {
                case "Equal": return (l, r) => $"{l} = {r}";
                case "NotEqual": return (l, r) => $"{l} != {r}";
                case "LessThan": return (l, r) => $"{l} < {r}";
                case "LessThanOrEqual": return (l, r) => $"{l} <= {r}";
                case "GreaterThan": return (l, r) => $"{l} > {r}";
                case "GreaterThanOrEqual": return (l, r) => $"{l} >= {r}";
                case "And": return (l, r) => $"({l} AND {r})";
                case "AndAlso": return (l, r) => $"({l} AND {r})";
                case "Or": return (l, r) => $"({l} OR {r})";
                case "Contains": return (l, r) => $"CONTAINS({l}, {r}, true)";
                case "StartsWith": return (l, r) => $"STARTSWITH({l}, {r}, true)";
                case "EndsWith": return (l, r) => $"ENDSWITH({l}, {r}, true)";
                case "Equals": return (l, r) => $"STRINGEQUALS({l}, {r}, true)";
                case "Constant": return (l, r) => $"{r.ToLower()}";
                default:
                    throw new NotImplementedException($"Unhandled expression type \"{expressionType}\"");
            }
        }
    }
}