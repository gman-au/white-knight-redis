using System;
using System.Linq.Expressions;
using System.Reflection;

namespace White.Knight.Redis.Extensions
{
    public static class ClassEx
    {
        public static PropertyInfo ExtractPropertyInfo<T>(Expression fieldBody)
        {
            if (fieldBody == null) return null;

            var entityType = typeof(T);

            PropertyInfo propertyInfo = null;

            if (fieldBody is LambdaExpression lambdaExpression)
                return ExtractPropertyInfo<T>(lambdaExpression.Body);

            if (fieldBody is MemberExpression memberExpression)
            {
                var member =
                    memberExpression
                        .Member;

                propertyInfo =
                    entityType
                        .GetProperty(member.Name);

                return propertyInfo;
            }

            if (fieldBody is UnaryExpression unaryExpression)
                if (unaryExpression.NodeType == ExpressionType.Convert)
                    if (unaryExpression.Operand is MemberExpression convertMemberExpression)
                    {
                        var member =
                            convertMemberExpression
                                .Member;

                        propertyInfo =
                            entityType
                                .GetProperty(member.Name);

                        return propertyInfo;
                    }

            if (propertyInfo == null)
                throw new Exception($"Could not apply iterative field strategy to object of type {fieldBody.NodeType}");

            return propertyInfo;
        }
    }
}