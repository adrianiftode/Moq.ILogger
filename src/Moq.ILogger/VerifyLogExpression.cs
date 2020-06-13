using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Moq
{
    internal class VerifyLogExpression
    {
        public VerifyLogExpressionArgs Args { get; private set; }
        public Expression Message { get; private set; }
        public Expression Exception { get; private set; }
        public Expression EventId { get; private set; }
        public Expression MessageArgs { get; private set; }

        public static VerifyLogExpression From(Expression expression) =>
            new VerifyLogExpression
            {
                Exception = ExpressionInspector.GetArgExpression(expression, c => typeof(Exception).IsAssignableFrom(c.Type)),
                EventId = ExpressionInspector.GetArgExpressionOf<EventId>(expression),
                Message = ExpressionInspector.GetArgExpressionOf<string>(expression),
                MessageArgs = ExpressionInspector.GetArgExpressionOf<object[]>(expression),
                Args = VerifyLogExpressionArgs.From(expression)
            };
    }
}
