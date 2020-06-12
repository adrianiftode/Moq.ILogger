using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Moq
{
    internal class LogArgs
    {
        public LogLevel LogLevel { get; private set; }
        public string Message { get; private set; }
        public Exception Exception { get; private set; }
        public EventId EventId { get; private set; }
        public object[] MessageArgs { get; private set; }

        public static LogArgs From(Expression expression) => new LogArgs
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
            if (methodCall == null)
            {
                throw new InvalidOperationException("Only ILogger extensions");
            }
            var name = methodCall.Method.Name;
            var logLevel = name switch
            {
                "LogDebug" => LogLevel.Debug,
                "LogInformation" => LogLevel.Information,
                "LogWarning" => LogLevel.Warning,
                "LogError" => LogLevel.Error,
                "LogTrace" => LogLevel.Trace,
                _ => throw new InvalidOperationException("Only ILogger extensions"),
            };
            return logLevel;
        }

        private static EventId GetEventId(Expression expression)
        {
            var methodCall = (MethodCallExpression)((LambdaExpression)expression).Body;
            var argExpression = methodCall.Arguments.FirstOrDefault(c => c.Type == typeof(EventId)) as ConstantExpression;
            if (argExpression == null)
            {
                return default;
            }
            return (EventId)argExpression.Value;
        }

        private static T GetArgOf<T>(Expression expression) where T : class
        {
            var methodCall = (MethodCallExpression)((LambdaExpression)expression).Body;
            var argExpression = methodCall.Arguments.FirstOrDefault(c => c.Type == typeof(T)) as ConstantExpression;
            var arg = argExpression?.Value as T;
            return arg;
        }

        private static Exception GetException(Expression expression)
        {
            var methodCall = (MethodCallExpression)((LambdaExpression)expression).Body;
            var argExpression = methodCall.Arguments.FirstOrDefault(c => typeof(Exception).IsAssignableFrom(c.Type));
            if (argExpression is ConstantExpression constantExceptionExpression)
            {
                return constantExceptionExpression.Value as Exception;
            }
            if (argExpression is NewExpression newExceptionExpression)
            {
                return Expression.Lambda<Func<Exception>>(newExceptionExpression).Compile().Invoke();
            }
            return null;
        }
    }
}
