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
    public static class VerifyLogExtensions
    {
        private const string NullMessageFormatted = "[null]";

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the ILogger mock.
        /// </summary>
        /// <param name="loggerMock">The ILogger mock object.</param>
        /// <param name="expression">Expression to verify.</param>
        /// <exception cref="VerifyLogException">
        /// The invocation was not performed on the mock.
        /// </exception>
        public static void VerifyLog(this Mock<ILogger> loggerMock, Expression<Action<ILogger>> expression)
        {
            EnsureExpressionIsForLoggerExtensions(expression);

            var logArgsExpressions = VerifyLogArgsExpressions.From(expression);
            var verifyExpression = CreateMoqVerifyExpressionFrom<ILogger>(logArgsExpressions);
            try
            {
                loggerMock.Verify(verifyExpression);
            }
            catch (MockException ex)
            {
                throw new VerifyLogException(BuildExceptionMessage(ex, expression, logArgsExpressions.LogArgs), ex);
            }
        }

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the generic ILogger mock.
        /// </summary>
        /// <param name="loggerMock">The generic ILogger mock object.</param>
        /// <param name="expression">Expression to verify.</param>
        /// <exception cref="VerifyLogException">
        /// The invocation was not performed on the mock.
        /// </exception>
        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, Expression<Action<ILogger>> expression)
        {
            EnsureExpressionIsForLoggerExtensions(expression);

            var logArgs = VerifyLogArgs.From(expression);
            var logArgsExpressions = VerifyLogArgsExpressions.From(expression);
            var verifyExpression = CreateMoqVerifyExpressionFrom<ILogger<T>>(logArgsExpressions);
            try
            {
                loggerMock.Verify(verifyExpression);
            }
            catch (MockException ex)
            {
                throw new VerifyLogException(BuildExceptionMessage(ex, expression, logArgs), ex);
            }
        }

        private static Expression<Action<T>> CreateMoqVerifyExpressionFrom<T>(VerifyLogArgsExpressions logArgsExpressions)
        {
            var logLevelExpression = CreateLogLevelExpression(logArgsExpressions.LogArgs);
            var eventIdExpression = CreateEventIdExpression();
            var exceptionExpression = CreateExceptionExpression(logArgsExpressions.LogArgs, logArgsExpressions);
            var messageExpression = CreateMessageExpression(logArgsExpressions);
            var formatterExpression = CreateFormatterExpression();

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

        private static Expression CreateLogLevelExpression(VerifyLogArgs args) 
            => Expression.Constant(args.LogLevel);

        private static Expression CreateFormatterExpression()
        {
            var itIsAnyObjectExpression = BuildItIsAnyExpression<object>();
            var formatterExpression = Expression.Convert(itIsAnyObjectExpression, typeof(Func<It.IsAnyType, Exception, string>));
            return formatterExpression;
        }

        private static Expression CreateEventIdExpression()
            => BuildItIsAnyExpression<EventId>();

        private static Expression CreateExceptionExpression(VerifyLogArgs args, VerifyLogArgsExpressions expressions)
        {
            if (expressions.Exception == null)
            {
                return BuildItIsAnyExpression<Exception>();
            }

            if (args.Exception != null)
            {
                // build It.Is(CompareExceptions(exception)),
                var exceptionParam = Expression.Parameter(typeof(Exception));
                var exceptionConstantExpression = Expression.Constant(args.Exception, typeof(Exception));
                var compareExceptionsCallExpression = Expression.Call(typeof(VerifyLogExtensions), nameof(CompareExceptions), null, exceptionConstantExpression, exceptionParam);
                var compareExpression = Expression.Lambda<Func<Exception, bool>>(compareExceptionsCallExpression, exceptionParam);
                var compareExceptionQuoteExpression = Expression.Quote(compareExpression);
                var itIsExceptionExpression = Expression.Call(typeof(It), "Is", new[] { typeof(Exception) }, compareExceptionQuoteExpression);
                return itIsExceptionExpression;
            }

            return expressions.Exception ?? BuildItIsAnyExpression<Exception>();
        }

        private static MethodCallExpression CreateMessageExpression(VerifyLogArgsExpressions logArgsExpressions)
        {
            var messageArg = logArgsExpressions.Message;

            var vParam = Expression.Parameter(typeof(object), "v");
            var tParam = Expression.Parameter(typeof(Type), "t");


            Expression<Func<object, Type, bool>> compareExpression = null;
            if (messageArg is ConstantExpression messageArgConstant)
            {
                var messageConstantExpression = Expression.Constant(messageArgConstant.Value, typeof(string));
                var compareMessagesCallExpression = Expression.Call(typeof(VerifyLogExtensions), nameof(CompareMessages), null, messageConstantExpression, vParam);
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

        private static void EnsureExpressionIsForLoggerExtensions(Expression expression)
        {
            var methodCall = (expression as LambdaExpression)?.Body as MethodCallExpression;
            var methodIsaMsLoggerExtensions = methodCall?.Method.ReflectedType == typeof(LoggerExtensions);
            var methodName = methodCall?.Method.Name;
            var supportedMethods = new[]
            {
                nameof(LoggerExtensions.LogCritical),
                nameof(LoggerExtensions.LogDebug),
                nameof(LoggerExtensions.LogError),
                nameof(LoggerExtensions.LogInformation),
                nameof(LoggerExtensions.LogWarning),
                nameof(LoggerExtensions.LogTrace)
            };

            if (!methodIsaMsLoggerExtensions || methodName is null || !supportedMethods.Contains(methodName))
            {
                var message = "Moq.ILogger supports only the extensions " +
                              "defined in the Microsoft.Extensions.Logging package " +
                              "and that are specifically defined for " +
                              $"LogXXX use cases ({string.Join(", ", supportedMethods)}).";

                if (!string.IsNullOrEmpty(methodName))
                {
                    message += $" The resolved method `{methodName}` in the verify expression is not one of these.";
                }
                else
                {
                    message += " A method name could not be resolved from the verify expression.";
                }

                throw new NotSupportedException(message);
            }
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
            => Expression.Call(typeof(It), "IsAny", new[] { typeof(T) });

        private static string BuildExceptionMessage(MockException ex, Expression expression, VerifyLogArgs args)
        {
            return BuildExceptionMessage(ex, args.LogLevel, args.Message);
        }

        private static string BuildExceptionMessage(MockException ex, LogLevel level, string message)
            => $"Expected an invocation on the .Log{level}(\"{message}\"), but was never performed." +
                                $"{Environment.NewLine}" +
                                $"{Environment.NewLine}" +
                                $"{ex}";
    }
}
