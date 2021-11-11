using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Moq
{
    internal class VerifyLogExpression
    {
        public VerifyLogExpressionArgs Args { get; private set; }
        public Expression MessageExpression { get; private set; }
        public Expression ExceptionExpression { get; private set; }
        public Expression EventIdExpression { get; private set; }
        public Expression MessageArgsExpression { get; private set; }
        public bool HasExpectedMessageArgs => MessageArgsExpression is MethodCallExpression || (MessageArgsExpression as NewArrayExpression)?.Expressions.Count > 0;

        public static VerifyLogExpression From(Expression expression) =>
            new VerifyLogExpression
            {
                ExceptionExpression = ExpressionInspector.GetArgExpression(expression, c => typeof(Exception).IsAssignableFrom(c.Type)),
                EventIdExpression = ExpressionInspector.GetArgExpressionOf<EventId>(expression),
                MessageExpression = ExpressionInspector.GetArgExpressionOf<string>(expression),
                MessageArgsExpression = ExpressionInspector.GetArgExpressionOf<object[]>(expression),
                Args = VerifyLogExpressionArgs.From(expression)
            };
    }
}
