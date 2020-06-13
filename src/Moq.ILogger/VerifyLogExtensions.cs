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

            var verifyLogExpression = VerifyLogExpression.From(expression);
            var verifyExpression = CreateMoqVerifyExpressionFrom<ILogger>(verifyLogExpression);
            try
            {
                loggerMock.Verify(verifyExpression);
            }
            catch (MockException ex)
            {
                throw new VerifyLogException(BuildExceptionMessage(ex, expression, verifyLogExpression.Args), ex);
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

            var verifyLogExpression = VerifyLogExpression.From(expression);
            var verifyExpression = CreateMoqVerifyExpressionFrom<ILogger<T>>(verifyLogExpression);
            try
            {
                loggerMock.Verify(verifyExpression);
            }
            catch (MockException ex)
            {
                throw new VerifyLogException(BuildExceptionMessage(ex, expression, verifyLogExpression.Args), ex);
            }
        }

        private static Expression<Action<T>> CreateMoqVerifyExpressionFrom<T>(VerifyLogExpression verifyLogExpression)
        {
            var logLevelExpression = CreateLogLevelExpression(verifyLogExpression);
            var eventIdExpression = CreateEventIdExpression();
            var exceptionExpression = CreateExceptionExpression(verifyLogExpression);
            var messageExpression = CreateMessageExpression(verifyLogExpression);
            var formatterExpression = CreateFormatterExpression();

            var loggerParameter = Expression.Parameter(typeof(T), "logger");
            var logMethodInfo = typeof(ILogger).GetMethod(nameof(ILogger.Log))!.MakeGenericMethod(typeof(It.IsAnyType));
            var logCallExpression = Expression.Call(loggerParameter, logMethodInfo,
                logLevelExpression,
                eventIdExpression,
                messageExpression,
                exceptionExpression,
                formatterExpression);

            var verifyExpression = Expression.Lambda<Action<T>>(logCallExpression, loggerParameter);
            return verifyExpression;
        }

        private static Expression CreateLogLevelExpression(VerifyLogExpression verifyLogExpression)
            => Expression.Constant(verifyLogExpression.Args.LogLevel);

        private static Expression CreateFormatterExpression()
        {
            var itIsAnyObjectExpression = BuildItIsAnyExpression<object>();
            var formatterExpression = Expression.Convert(itIsAnyObjectExpression, typeof(Func<It.IsAnyType, Exception, string>));
            return formatterExpression;
        }

        private static Expression CreateEventIdExpression()
            => BuildItIsAnyExpression<EventId>();

        private static Expression CreateExceptionExpression(VerifyLogExpression verifyLogExpression)
        {
            if (verifyLogExpression.ExceptionExpression == null)
            {
                return BuildItIsAnyExpression<Exception>();
            }

            // an Expression arg is given, create an It.Is<Exception>(e => Compare(e, arg))
            var exception = verifyLogExpression.Args.Exception;
            if (exception != null)
            {
                // build It.Is(CompareExceptions(exception)),
                var exceptionParam = Expression.Parameter(typeof(Exception));
                var exceptionConstantExpression = Expression.Constant(exception, typeof(Exception));
                var compareExceptionsCallExpression = Expression.Call(typeof(VerifyLogExtensions), nameof(CompareExceptions), null, exceptionConstantExpression, exceptionParam);
                var compareExpression = Expression.Lambda<Func<Exception, bool>>(compareExceptionsCallExpression, exceptionParam);
                var compareExceptionQuoteExpression = Expression.Quote(compareExpression);
                var itIsExceptionExpression = Expression.Call(typeof(It), "Is", new[] { typeof(Exception) }, compareExceptionQuoteExpression);
                return itIsExceptionExpression;
            }

            return verifyLogExpression.ExceptionExpression ?? BuildItIsAnyExpression<Exception>();
        }

        private static MethodCallExpression CreateMessageExpression(VerifyLogExpression verifyLogExpression)
        {
            var messageExpression = verifyLogExpression.MessageExpression;

            var vParam = Expression.Parameter(typeof(object), "v");
            var tParam = Expression.Parameter(typeof(Type), "t");

            Expression<Func<object, Type, bool>> compareExpression = null;
            switch (messageExpression)
            {
                case ConstantExpression messageArgConstant:
                    {
                        var messageConstantExpression = Expression.Constant(messageArgConstant.Value, typeof(string));
                        var compareMessagesCallExpression = Expression.Call(typeof(VerifyLogExtensions), nameof(CompareMessages), null, messageConstantExpression, vParam);
                        compareExpression = Expression.Lambda<Func<object, Type, bool>>(compareMessagesCallExpression, vParam, tParam);
                        break;
                    }
                case MethodCallExpression methodCallExpression:
                    {
                        var methodName = methodCallExpression.Method.Name;
                        switch (methodName)
                        {
                            // It.IsAny<string>()
                            case "IsAny":
                                {
                                    // build (v, t) => true
                                    var trueExpression = Expression.Constant(true);
                                    compareExpression = Expression.Lambda<Func<object, Type, bool>>(trueExpression, vParam, tParam);
                                    break;
                                }
                            // It.Is<string>(msg => msg.Contains("Test"))
                            case "Is":
                                {
                                    // build It.Is<It.IsAnyType>((v, t) => messagePredicate(v.ToString())
                                    var messagePredicate = ((UnaryExpression)methodCallExpression.Arguments.First()).Operand;
                                    if (messagePredicate is Expression<Func<string, bool>> stringMessagePredicate)
                                    {
                                        var vParamToStringExpression = CreatevParamToStringExpression();
                                        var invokeStringMessagePredicateExpression = Expression.Invoke(stringMessagePredicate, vParamToStringExpression);
                                        compareExpression = Expression.Lambda<Func<object, Type, bool>>(invokeStringMessagePredicateExpression, vParam, tParam);
                                    }

                                    break;
                                }
                            // It.IsNotNull<string>())
                            case "IsNotNull":
                                {
                                    // build (v, t) => v.ToString() != "[null]"
                                    var vParamToStringExpression = CreatevParamToStringExpression();
                                    var nullConstantExpression = Expression.Constant(NullMessageFormatted);
                                    var notNullConstantExpression = Expression.NotEqual(vParamToStringExpression, nullConstantExpression);
                                    compareExpression = Expression.Lambda<Func<object, Type, bool>>(notNullConstantExpression, vParam, tParam);
                                    break;
                                }
                            // It.IsRegex(pattern))
                            case "IsRegex":
                                {
                                    // build (v, t) => Regex.IsMatch(v.ToString(), pattern, RegexOptions.IgnoreCase)
                                    var pattern = ((ConstantExpression)methodCallExpression.Arguments[0]).Value;
                                    var patternConstantExpression = Expression.Constant(pattern);
                                    var regexOptionsConstantExpression = Expression.Constant(RegexOptions.IgnoreCase);
                                    var vParamToStringExpression = CreatevParamToStringExpression();
                                    var callRegexIsMatchExpression = Expression.Call(typeof(Regex), nameof(Regex.IsMatch), null, vParamToStringExpression, patternConstantExpression, regexOptionsConstantExpression);
                                    compareExpression = Expression.Lambda<Func<object, Type, bool>>(callRegexIsMatchExpression, vParam, tParam);
                                    break;
                                }
                            // GetSomeMessage(a, b, c, ....)
                            default:
                            {
                                //build (v, t) => Compare(v.ToString(), GetSomeMessage(a, b, c, ....))
                                compareExpression = CreateCompareLambdaFrom(methodCallExpression);
                                break;
                            }
                        }
                        break;
                    }
                default:
                    {
                        // build (v, t) => Compare(v.ToString(), /*inject the expression here*/)
                        compareExpression = CreateCompareLambdaFrom(messageExpression);
                        break;
                    }
            }

            var compareMessageQuoteExpression = Expression.Quote(compareExpression);
            var itIsMessageExpression = Expression.Call(typeof(It), "Is", new Type[] { typeof(It.IsAnyType) }, compareMessageQuoteExpression);
            return itIsMessageExpression;

            Expression<Func<object, Type, bool>> CreateCompareLambdaFrom(Expression methodCallExpression)
            {
                var vParamToStringExpression = Expression.Call(vParam, typeof(object).GetMethod(nameof(ToString))!);
                var compareMessagesCallExpression = Expression.Call(typeof(VerifyLogExtensions), nameof(CompareMessages), null,
                    vParamToStringExpression, methodCallExpression);
                compareExpression = Expression.Lambda<Func<object, Type, bool>>(compareMessagesCallExpression, vParam, tParam);
                return compareExpression;
            }

            MethodCallExpression CreatevParamToStringExpression()
            {
                var vParamToStringExpression = Expression.Call(vParam, typeof(object).GetMethod(nameof(ToString))!);
                return vParamToStringExpression;
            }
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

        private static string BuildExceptionMessage(MockException ex, Expression expression, VerifyLogExpressionArgs args)
            => BuildExceptionMessage(ex, args.LogLevel, args.Message);

        private static string BuildExceptionMessage(MockException ex, LogLevel level, string message)
            => $"Expected an invocation on the .Log{level}(\"{message}\"), but was never performed." +
                                $"{Environment.NewLine}" +
                                $"{Environment.NewLine}" +
                                $"{ex}";
    }
}
