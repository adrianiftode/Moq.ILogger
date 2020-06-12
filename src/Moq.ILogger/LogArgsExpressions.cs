using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Moq
{
    internal class LogArgsExpressions
    {
        public Expression Message { get; private set; }
        public Expression Exception { get; private set; }
        public Expression EventId { get; private set; }
        public Expression MessageArgs { get; private set; }

        public static LogArgsExpressions From(Expression expression)
        {
            var methodCall = (MethodCallExpression)((LambdaExpression)expression).Body;
            return new LogArgsExpressions
            {
                Exception = methodCall.Arguments.FirstOrDefault(c => typeof(Exception).IsAssignableFrom(c.Type)),
                EventId = methodCall.Arguments.FirstOrDefault(c => c.Type == typeof(EventId)),
                Message = methodCall.Arguments.FirstOrDefault(c => c.Type == typeof(string)),
                MessageArgs = methodCall.Arguments.FirstOrDefault(c => c.Type == typeof(object[]))
            };
        }
    }
}
