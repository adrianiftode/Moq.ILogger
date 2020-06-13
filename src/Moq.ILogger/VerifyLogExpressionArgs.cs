using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Moq
{
    internal class VerifyLogExpressionArgs
    {
        public LogLevel LogLevel { get; private set; }
        public string Message { get; private set; }
        public Exception Exception { get; private set; }
        public EventId EventId { get; private set; }
        public object[] MessageArgs { get; private set; }

        public static VerifyLogExpressionArgs From(Expression expression) => new VerifyLogExpressionArgs
        {
            LogLevel = GetLogLevelFrom(expression),
            Message = ExpressionInspector.GetArgOf<string>(expression),
            MessageArgs = ExpressionInspector.GetArgOf<object[]>(expression),
            Exception = GetException(expression),
            EventId = GetEventId(expression)
        };

        private static LogLevel GetLogLevelFrom(Expression expression)
        {
            var methodCall = (MethodCallExpression)((LambdaExpression)expression).Body;
            var name = methodCall.Method.Name;
            var logLevel = name switch
            {
                "LogDebug" => LogLevel.Debug,
                "LogInformation" => LogLevel.Information,
                "LogWarning" => LogLevel.Warning,
                "LogError" => LogLevel.Error,
                "LogTrace" => LogLevel.Trace,
                "LogCritical" => LogLevel.Critical,
                _ => throw new NotSupportedException($"A LogLevel for method {methodCall} could not be resolved.")
            };
            return logLevel;
        }

        private static EventId GetEventId(Expression expression) 
            => !(ExpressionInspector.GetArgExpression(expression, c => c.Type == typeof(EventId)) is ConstantExpression eventIdArgExpression)
                ? default
                : (EventId) eventIdArgExpression.Value;
        private static Exception GetException(Expression expression)
            => ExpressionInspector.GetArgExpression(expression, c => typeof(Exception).IsAssignableFrom(c.Type)) switch
            {
                ConstantExpression constantExceptionExpression => constantExceptionExpression.Value as Exception,
                NewExpression newExceptionExpression => Expression.Lambda<Func<Exception>>(newExceptionExpression)
                    .Compile()
                    .Invoke(),
                _ => null
            };
    }
}
