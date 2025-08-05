using System;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using White.Knight.Domain;
using White.Knight.Domain.Exceptions;
using White.Knight.Interfaces;
using White.Knight.Interfaces.Command;
using ClassEx = White.Knight.Redis.Extensions.ClassEx;

namespace White.Knight.Redis.Translator
{
    public class RedisCommandTranslator<TD, TResponse>(
        ILoggerFactory loggerFactory = null
    )
        : ICommandTranslator<TD, RedisTranslationResult> where TD : new()
    {
        private readonly ILogger _logger =
            (loggerFactory ?? new NullLoggerFactory())
            .CreateLogger<RedisCommandTranslator<TD, TResponse>>();

        public RedisTranslationResult Translate(ISingleRecordCommand<TD> command)
        {
            var query = string.Empty;

            _logger
                .LogDebug("Translated Query: [{query}]", query);

            return new RedisTranslationResult
            {
                Query = query
            };
        }

        public RedisTranslationResult Translate<TP>(IQueryCommand<TD, TP> command)
        {
            var specification = command.Specification;
            var pagingOptions = command.PagingOptions;

            try
            {
                var query = Translate(specification);

                _logger
                    .LogDebug("Translated Query: ({specification}) [{query}]", specification.GetType().Name, query);

                var page = pagingOptions?.Page;
                var pageSize = pagingOptions?.PageSize;

                var sortDescending = pagingOptions?.Descending;

                var sort =
                    ClassEx
                        .ExtractPropertyInfo<TD>(pagingOptions?.OrderBy);

                return new RedisTranslationResult
                {
                    Query = query,
                    Page = page,
                    PageSize = pageSize,
                    SortDescending = sortDescending,
                    SortByField = sort?.Name
                };
            }
            catch (Exception e) when (e is NotImplementedException or UnparsableSpecificationException)
            {
                _logger
                    .LogDebug("Error translating Query: ({specification})", specification.GetType().Name);

                throw;
            }
        }

        public RedisTranslationResult Translate(IUpdateCommand<TD> command)
        {
            var query = string.Empty;

            _logger
                .LogDebug("Translated Query: [{query}]", query);

            return new RedisTranslationResult
            {
                Query = query
            };
        }

        private string Translate(Specification<TD> spec)
        {
            return spec switch
            {
                SpecificationByAll<TD> => " * ",
                SpecificationByNone<TD> => " FALSE ",
                SpecificationByEquals<TD, string> eq => $"@{GetPropertyExpressionName(eq.Property.Body)}:{eq.Value}",
                SpecificationByEquals<TD, int> eq => $"@{GetPropertyExpressionName(eq.Property.Body)}:[{eq.Value} {eq.Value}]",
                SpecificationByEquals<TD, Guid> eq => $"@{GetPropertyExpressionName(eq.Property.Body)}:{eq.Value}",
                SpecificationByAnd<TD> and => $" {Translate(and.Left)} {Translate(and.Right)} ",
                SpecificationByOr<TD> => throw new UnparsableSpecificationException(),
                SpecificationByTextStartsWith<TD> text => $"@{GetPropertyExpressionName(text.Property.Body)}:{text.Value}*",
                SpecificationByTextEndsWith<TD> text => $"@{GetPropertyExpressionName(text.Property.Body)}:*{text.Value}",
                SpecificationByTextContains<TD> text => $"@{GetPropertyExpressionName(text.Property.Body)}:*{text.Value}*",
                SpecificationThatIsNotCompatible<TD> => throw new UnparsableSpecificationException(),
                _ => throw new NotImplementedException()
            };
        }

        private string GetPropertyExpressionName(Expression ex)
        {
            if (ex is MemberExpression mex)
            {
                return mex.Member.Name;
            }

            return null;
        }
    }
}