using System;
using System.Linq;
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Moq
{
    internal static class ExpressionInspector
    {
        internal static T GetArgOf<T>(Expression expression) where T : class
            => (GetArgExpression(expression, c => c.Type == typeof(T)) as ConstantExpression)?.Value as T;

        internal static Expression GetArgExpression(Expression expression, Func<Expression, bool> argPredicate)
        {
            var methodCall = (MethodCallExpression)((LambdaExpression)expression).Body;
            var argExpression = methodCall.Arguments.FirstOrDefault(argPredicate);
            return argExpression;
        }

        internal static Expression GetArgExpressionOf<T>(Expression expression)
            => GetArgExpression(expression, c => c.Type == typeof(T));
    }
}