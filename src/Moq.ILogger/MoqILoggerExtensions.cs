//https://github.com/dotnet/runtime/blob/e3ffd343ad5bd3a999cb9515f59e6e7a777b2c34/src/libraries/Microsoft.Extensions.Logging.Abstractions/src/LoggerExtensions.cs
using Microsoft.Extensions.Logging;
using Moq.Internal;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Moq
{
    public static class MoqILoggerExtensions
    {
        private const string NullMessageFromatted = "[null]";

        public static void Verify(this Mock<ILogger> loggerMock, LogLevel logLevel, string message)
        {
            try
            {
                loggerMock.Verify(logger =>
                                 logger.Log(logLevel,
                                        It.IsAny<EventId>(),
                                        It.Is<It.IsAnyType>((v, t) => CompareMessages(message, v)),
                                        It.IsAny<Exception>(),
                                        (Func<It.IsAnyType, Exception, string>)It.IsAny<object>())
                );
            }
            catch (MockException ex)
            {
                throw new ILoggerMockException(

                    $"Expected an invocation on the .Log{logLevel}(\"{message}\"), but was never performed." +
                    $"{Environment.NewLine}" +
                    $"{Environment.NewLine}" +
                    $"{ex}", ex);
            }
        }

        public static void Verify(this Mock<ILogger> loggerMock, LogLevel logLevel, Exception exception, string message)
        {
            try
            {
                Expression<Action<ILogger>> expression = logger => logger.Log(logLevel,
                                                           It.IsAny<EventId>(),
                                                           It.Is<It.IsAnyType>((v, t) => CompareMessages(message, v)),
                                                           It.Is(CompareExceptions(exception)),
                                                           (Func<It.IsAnyType, Exception, string>)It.IsAny<object>());
                loggerMock.Verify(expression);
            }
            catch (MockException ex)
            {
                throw new ILoggerMockException(

                    $"Expected an invocation on the .Log{logLevel}(\"{message}\"), but was never performed." +
                    $"{Environment.NewLine}" +
                    $"{Environment.NewLine}" +
                    $"{ex}", ex);
            }
        }

        public static void VerifyLog(this Mock<ILogger> loggerMock, Expression<Action<ILogger>> expression)
        {
            var loggerParameter = Expression.Parameter(typeof(ILogger), "logger");
            var message = GetMessage(expression);
            var logLevel = GetLogLevel(expression);
            var logLevelExpression = Expression.Constant(logLevel);

            var itIsAnyEventIdExpression = Expression.Call(typeof(It), "IsAny", new Type[] { typeof(EventId) });
            var itIsAnyExceptionExpression = Expression.Call(typeof(It), "IsAny", new Type[] { typeof(Exception) });
            var exceptionExpression = GetExceptionExpression(expression);
            var messageExpression = CreateMessageExpression(expression);
            var itIsAnyObjectExpression = Expression.Call(typeof(It), "IsAny", new Type[] { typeof(object) });
            var formatterExpression = Expression.Convert(itIsAnyObjectExpression, typeof(Func<It.IsAnyType, Exception, string>));


            var logMethodInfo = typeof(ILogger).GetMethod("Log").MakeGenericMethod(typeof(It.IsAnyType));
            var logCallExpression = Expression.Call(loggerParameter, logMethodInfo,
                logLevelExpression,
                itIsAnyEventIdExpression,
                messageExpression,
                exceptionExpression ?? itIsAnyExceptionExpression,
                formatterExpression
                );
            Expression<Action<ILogger>> verifyExpression = Expression.Lambda<Action<ILogger>>(logCallExpression, loggerParameter);
            try
            {
                loggerMock.Verify(verifyExpression);
            }
            catch (MockException ex)
            {
                throw new ILoggerMockException(

                    $"Expected an invocation on the .Log{logLevel}(\"{message}\"), but was never performed." +
                    $"{Environment.NewLine}" +
                    $"{Environment.NewLine}" +
                    $"{ex}", ex);
            }
        }

        private class LogArgsExpressions
        {
            public Expression Message { get; set; }
            public Expression Exception { get; set; }
            public Expression EventId { get; set; }
            public Expression Args { get; set; }
        }

        private static LogArgsExpressions GetLogArgsExpressions(Expression<Action<ILogger>> expression)
        {
            var methodCall = (MethodCallExpression)expression.Body;
            return new LogArgsExpressions
            {
                Exception = methodCall.Arguments.FirstOrDefault(c => typeof(Exception).IsAssignableFrom(c.Type)),
                EventId = methodCall.Arguments.FirstOrDefault(c => c.Type == typeof(EventId)),
                Message = methodCall.Arguments.FirstOrDefault(c => c.Type == typeof(string)),
                Args = methodCall.Arguments.FirstOrDefault(c => c.Type == typeof(object[]))
            };
        }

        private static Expression GetExceptionExpression(Expression<Action<ILogger>> expression)
        {
            var methodCall = (MethodCallExpression)expression.Body;
            var exceptionExpression = methodCall.Arguments.FirstOrDefault(c => typeof(Exception).IsAssignableFrom(c.Type));
            return exceptionExpression;
        }

        private static string GetMessage(Expression<Action<ILogger>> expression)
        {
            var methodCall = (MethodCallExpression)expression.Body;
            var messageExpression = methodCall.Arguments.First(c => c.Type == typeof(string)) as ConstantExpression;
            var message = messageExpression?.Value as string;
            return message;
        }

        private static LogLevel GetLogLevel(Expression<Action<ILogger>> expression)
        {
            var methodCall = expression.Body as MethodCallExpression;
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

        private static MethodCallExpression CreateMessageExpression(Expression<Action<ILogger>> expression)
        {
            var messageArg = GetLogArgsExpressions(expression).Message;

            var vParam = Expression.Parameter(typeof(object), "v");
            var tParam = Expression.Parameter(typeof(Type), "t");


            Expression<Func<object, Type, bool>> compareExpression = null;
            if (messageArg is ConstantExpression messageArgConstant)
            {
                var messageConstantExpression = Expression.Constant(messageArgConstant.Value, typeof(string));
                var compareMessagesCallExpression = Expression.Call(typeof(MoqILoggerExtensions), nameof(CompareMessages), null, messageConstantExpression, vParam);
                compareExpression = Expression.Lambda<Func<object, Type, bool>>(compareMessagesCallExpression, vParam, tParam);
            }
            else if (messageArg is MethodCallExpression methodCallExpression)
            {
                var methodName = methodCallExpression.Method.Name;
                if (methodName == "IsAny") // It.IsAny<string>()
                {
                    // build (v, t) => true
                    var trueExpression = Expression.Constant(true);
                    compareExpression = Expression.Lambda<Func<object, Type, bool>>(trueExpression, vParam, tParam);
                }
                else if (methodName == "Is") // It.Is<string>(msg => msg.Contains("Test"))
                {
                    // build It.Is<It.IsAnyType>((v, t) => messagePredicate(v.ToString())
                    var messagePredicate = ((UnaryExpression)methodCallExpression.Arguments.First()).Operand;
                    if (messagePredicate is Expression<Func<string, bool>> stringMessagePredicate)
                    {
                        var vParamToStringExpression = Expression.Call(vParam, typeof(object).GetMethod(nameof(object.ToString)));
                        var invokeStringMessagePredicateExpression = Expression.Invoke(stringMessagePredicate, vParamToStringExpression);
                        compareExpression = Expression.Lambda<Func<object, Type, bool>>(invokeStringMessagePredicateExpression, vParam, tParam);
                    }
                }
                else if (methodName == "IsNotNull") // It.IsNotNull<string>())
                {
                    // build (v, t) => v.ToString() != "[null]"
                    var vParamToStringExpression = Expression.Call(vParam, typeof(object).GetMethod(nameof(object.ToString)));
                    var nullConstantExpression = Expression.Constant(NullMessageFromatted);
                    var notNullConstantExpression = Expression.NotEqual(vParamToStringExpression, nullConstantExpression);
                    compareExpression = Expression.Lambda<Func<object, Type, bool>>(notNullConstantExpression, vParam, tParam);
                }
                else if (methodName == "IsRegex") // It.IsRegex(pattern))
                {
                    // build (v, t) => Regex.IsMatch(v.ToString(), pattern, RegexOptions.IgnoreCase)
                    var pattern = ((ConstantExpression)((MethodCallExpression)messageArg).Arguments[0]).Value;
                    var patternConstantExpression = Expression.Constant(pattern);
                    var regexOptionsConstantExpression = Expression.Constant(RegexOptions.IgnoreCase);
                    var vParamToStringExpression = Expression.Call(vParam, typeof(object).GetMethod(nameof(object.ToString)));
                    var callRegexIsMatchExpression = Expression.Call(typeof(Regex), nameof(Regex.IsMatch), null, vParamToStringExpression, patternConstantExpression, regexOptionsConstantExpression);
                    compareExpression = Expression.Lambda<Func<object, Type, bool>>(callRegexIsMatchExpression, vParam, tParam);
                }
            }

            var compareMessageQuoteExpression = Expression.Quote(compareExpression);
            var itIsMessageExpression = Expression.Call(typeof(It), "Is", new Type[] { typeof(It.IsAnyType) }, compareMessageQuoteExpression);
            return itIsMessageExpression;
        }

        private static Expression<Func<Exception, bool>> CompareExceptions(Exception exception)
            => c => c.GetType() == exception.GetType() && c.Message == exception.Message;
        private static bool CompareMessages(string message, object v)
        {
            if (message == null && v.ToString() == NullMessageFromatted)
            {
                return true;
            }

            if (message == null || v.ToString() == NullMessageFromatted)
            {
                return false;
            }

            return v.ToString().IsWildcardMatch(message);
        }
    }
}
