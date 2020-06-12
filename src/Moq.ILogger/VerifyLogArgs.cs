using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Moq
{
    internal class VerifyLogArgs
    {
        public LogLevel LogLevel { get; private set; }
        public string Message { get; private set; }
        public Exception Exception { get; private set; }
        public EventId EventId { get; private set; }
        public object[] MessageArgs { get; private set; }

        public static VerifyLogArgs From(Expression expression) => new VerifyLogArgs
        {
            LogLevel = GetLogLevelFrom(expression),
            Message = GetArgOf<string>(expression),
            MessageArgs = GetArgOf<object[]>(expression),
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
            => !(GetArgExpression(expression, c => c.Type == typeof(EventId)) is ConstantExpression eventIdArgExpression)
                ? default
                : (EventId) eventIdArgExpression.Value;

        private static T GetArgOf<T>(Expression expression) where T : class 
            => (GetArgExpression(expression, c => c.Type == typeof(T)) as ConstantExpression)?.Value as T;

        private static Exception GetException(Expression expression)
            => GetArgExpression(expression, c => typeof(Exception).IsAssignableFrom(c.Type)) switch
            {
                ConstantExpression constantExceptionExpression => constantExceptionExpression.Value as Exception,
                NewExpression newExceptionExpression => Expression.Lambda<Func<Exception>>(newExceptionExpression)
                    .Compile()
                    .Invoke(),
                _ => null
            };

        private static Expression GetArgExpression(Expression expression, Func<Expression, bool> argPredicate)
        {
            var methodCall = (MethodCallExpression)((LambdaExpression)expression).Body;
            var argExpression = methodCall.Arguments.FirstOrDefault(argPredicate);
            return argExpression;
        }
    }
}
