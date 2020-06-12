//https://github.com/dotnet/runtime/blob/e3ffd343ad5bd3a999cb9515f59e6e7a777b2c34/src/libraries/Microsoft.Extensions.Logging.Abstractions/src/LoggerExtensions.cs
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Moq
{
    /// <summary>
    /// Encapsulates a collection of extensions methods over Mock ILogger in order to define Moq Verify calls over the ILogger extensions
    /// </summary>
    public static class MoqILoggerExtensions
    {
        private const string NullMessageFormatted = "[null]";

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the ILogger mock.
        /// </summary>
        /// <param name="loggerMock">The ILogger mock object.</param>
        /// <param name="expression">Expression to verify.</param>
        /// <exception cref="LoggerMockException">
        /// The invocation was not performed on the mock.
        /// </exception>
        public static void VerifyLog(this Mock<ILogger> loggerMock, Expression<Action<ILogger>> expression)
        {
            var verifyExpression = CreateMoqVerifyExpressionFrom<ILogger>(expression);
            try
            {
                loggerMock.Verify(verifyExpression);
            }
            catch (MockException ex)
            {
                throw new LoggerMockException(BuildExceptionMessage(ex, expression), ex);
            }
        }

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the generic ILogger mock.
        /// </summary>
        /// <param name="loggerMock">The generic ILogger mock object.</param>
        /// <param name="expression">Expression to verify.</param>
        /// <exception cref="LoggerMockException">
        /// The invocation was not performed on the mock.
        /// </exception>
        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, Expression<Action<ILogger>> expression)
        {
            var verifyExpression = CreateMoqVerifyExpressionFrom<ILogger<T>>(expression);
            try
            {
                loggerMock.Verify(verifyExpression);
            }
            catch (MockException ex)
            {
                throw new LoggerMockException(BuildExceptionMessage(ex, expression), ex);
            }
        }

        private static Expression<Action<T>> CreateMoqVerifyExpressionFrom<T>(Expression expression)
        {
            var logLevelExpression = CreateLogLevelExpression(expression);
            var eventIdExpression = CreateEventIdExpression(expression);
            var exceptionExpression = CreateExceptionExpression(expression);
            var messageExpression = CreateMessageExpression(expression);
            var formatterExpression = CreateFormatterExpression(exceptionExpression);

            var loggerParameter = Expression.Parameter(typeof(T), "logger");
            var logMethodInfo = typeof(ILogger).GetMethod(nameof(ILogger.Log)).MakeGenericMethod(typeof(It.IsAnyType));
            var logCallExpression = Expression.Call(loggerParameter, logMethodInfo,
                logLevelExpression,
                eventIdExpression,
                messageExpression,
                exceptionExpression,
                formatterExpression);

            var verifyExpression = Expression.Lambda<Action<T>>(logCallExpression, loggerParameter);
            return verifyExpression;
        }

        private static Expression CreateLogLevelExpression(Expression expression)
        {
            var args = LogArgs.From(expression);
            var logLevel = args.LogLevel;
            return Expression.Constant(logLevel);
        }

        private static Expression CreateFormatterExpression(Expression expression)
        {
            var itIsAnyObjectExpression = BuildItIsAnyExpression<object>();
            var formatterExpression = Expression.Convert(itIsAnyObjectExpression, typeof(Func<It.IsAnyType, Exception, string>));
            return formatterExpression;
        }

        private static Expression CreateEventIdExpression(Expression expression)
            => BuildItIsAnyExpression<EventId>();

        private static Expression CreateExceptionExpression(Expression expression)
        {
            var expressions = LogArgsExpressions.From(expression);
            if (expressions.Exception == null)
            {
                return BuildItIsAnyExpression<Exception>();
            }

            var args = LogArgs.From(expression);
            if (args.Exception != null)
            {
                // build It.Is(CompareExceptions(exception)),
                var exceptionParam = Expression.Parameter(typeof(Exception));
                var exceptionConstantExpression = Expression.Constant(args.Exception, typeof(Exception));
                var compareExceptionsCallExpression = Expression.Call(typeof(MoqILoggerExtensions), nameof(CompareExceptions), null, exceptionConstantExpression, exceptionParam);
                var compareExpression = Expression.Lambda<Func<Exception, bool>>(compareExceptionsCallExpression, exceptionParam);
                var compareExceptionQuoteExpression = Expression.Quote(compareExpression);
                var itIsExceptionExpression = Expression.Call(typeof(It), "Is", new[] { typeof(Exception) }, compareExceptionQuoteExpression);
                return itIsExceptionExpression;
            }

            return expressions.Exception ?? BuildItIsAnyExpression<Exception>();
        }

        private static MethodCallExpression CreateMessageExpression(Expression expression)
        {
            var messageArg = LogArgsExpressions.From(expression).Message;

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
                    var nullConstantExpression = Expression.Constant(NullMessageFormatted);
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

        private static bool CompareExceptions(Exception exceptionA, Exception exceptionB)
        {
            if (exceptionA == null || exceptionB == null)
            {
                return false;
            }

            if (ReferenceEquals(exceptionA, exceptionB))
            {
                return true;
            }

            return exceptionA.Message == exceptionB.Message && exceptionA.GetType() == exceptionB.GetType();
        }

        private static bool CompareMessages(string message, object v)
        {
            if (message == null && v.ToString() == NullMessageFormatted)
            {
                return true;
            }

            if (message == null || v.ToString() == NullMessageFormatted)
            {
                return false;
            }

            return v.ToString().IsWildcardMatch(message);
        }

        private static Expression BuildItIsAnyExpression<T>()
            => Expression.Call(typeof(It), "IsAny", new Type[] { typeof(T) });

        private static string BuildExceptionMessage(MockException ex, Expression expression)
        {
            var args = LogArgs.From(expression);
            return BuildExceptionMessage(ex, args.LogLevel, args.Message);
        }

        private static string BuildExceptionMessage(MockException ex, LogLevel level, string message)
            => $"Expected an invocation on the .Log{level}(\"{message}\"), but was never performed." +
                                $"{Environment.NewLine}" +
                                $"{Environment.NewLine}" +
                                $"{ex}";
    }
}
