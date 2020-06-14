using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
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
        private const string OriginalFormat = "{OriginalFormat}";

        #region VerifyLog API
        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the ILogger mock.
        /// </summary>
        /// <param name="loggerMock">The generic ILogger mock object.</param>
        /// <param name="expression">Expression to verify.</param>
        /// <param name="failMessage">Message to show if verification fails.</param>
        /// <exception cref="VerifyLogException">
        /// The invocation was not performed on the ILogger mock.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The invocation expression was not defined for one of the logging extensions as defined in the <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions"><see cref="LoggerExtensions"/> class</see> from the <see href="https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/">Microsoft.Extensions.Logging.Abstractions</see> package.
        /// </exception>
        public static void VerifyLog(this Mock<ILogger> loggerMock, Expression<Action<ILogger>> expression, string failMessage)
            => Verify(loggerMock, expression, null, null, failMessage);

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the ILogger mock.
        /// </summary>
        /// <param name="loggerMock">The generic ILogger mock object.</param>
        /// <param name="expression">Expression to verify.</param>
        /// <param name="times">The number of times a method is expected to be called.</param>
        /// <exception cref="VerifyLogException">
        /// The invocation was not performed on the ILogger mock.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The invocation expression was not defined for one of the logging extensions as defined in the <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions"><see cref="LoggerExtensions"/> class</see> from the <see href="https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/">Microsoft.Extensions.Logging.Abstractions</see> package.
        /// </exception>
        public static void VerifyLog(this Mock<ILogger> loggerMock, Expression<Action<ILogger>> expression, Times times)
            => Verify(loggerMock, expression, times, null, null);

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the ILogger mock.
        /// </summary>
        /// <param name="loggerMock">The generic ILogger mock object.</param>
        /// <param name="expression">Expression to verify.</param>
        /// <param name="times">The number of times a method is expected to be called.</param>
        /// <param name="failMessage">Message to show if verification fails.</param>
        /// <exception cref="VerifyLogException">
        /// The invocation was not performed on the ILogger mock.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The invocation expression was not defined for one of the logging extensions as defined in the <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions"><see cref="LoggerExtensions"/> class</see> from the <see href="https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/">Microsoft.Extensions.Logging.Abstractions</see> package.
        /// </exception>
        public static void VerifyLog(this Mock<ILogger> loggerMock, Expression<Action<ILogger>> expression, Times times, string failMessage)
            => Verify(loggerMock, expression, times, null, failMessage);

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the ILogger mock.
        /// </summary>
        /// <param name="loggerMock">The generic ILogger mock object.</param>
        /// <param name="expression">Expression to verify.</param>
        /// <param name="times">The number of times a method is expected to be called.</param>
        /// <exception cref="VerifyLogException">
        /// The invocation was not performed on the ILogger mock.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The invocation expression was not defined for one of the logging extensions as defined in the <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions"><see cref="LoggerExtensions"/> class</see> from the <see href="https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/">Microsoft.Extensions.Logging.Abstractions</see> package.
        /// </exception>
        public static void VerifyLog(this Mock<ILogger> loggerMock, Expression<Action<ILogger>> expression, Func<Times> times)
            => Verify(loggerMock, expression, null, times, null);

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the ILogger mock.
        /// </summary>
        /// <param name="loggerMock">The generic ILogger mock object.</param>
        /// <param name="expression">Expression to verify.</param>
        /// <param name="times">The number of times a method is expected to be called.</param>
        /// <param name="failMessage">Message to show if verification fails.</param>
        /// <exception cref="VerifyLogException">
        /// The invocation was not performed on the ILogger mock.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The invocation expression was not defined for one of the logging extensions as defined in the <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions"><see cref="LoggerExtensions"/> class</see> from the <see href="https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/">Microsoft.Extensions.Logging.Abstractions</see> package.
        /// </exception>
        public static void VerifyLog(this Mock<ILogger> loggerMock, Expression<Action<ILogger>> expression, Func<Times> times, string failMessage)
            => Verify(loggerMock, expression, null, times, failMessage);

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the ILogger mock.
        /// </summary>
        /// <param name="loggerMock">The generic ILogger mock object.</param>
        /// <param name="expression">Expression to verify.</param>
        /// <exception cref="VerifyLogException">
        /// The invocation was not performed on the ILogger mock.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The invocation expression was not defined for one of the logging extensions as defined in the <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions"><see cref="LoggerExtensions"/> class</see> from the <see href="https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/">Microsoft.Extensions.Logging.Abstractions</see> package.
        /// </exception>
        public static void VerifyLog(this Mock<ILogger> loggerMock, Expression<Action<ILogger>> expression)
            => Verify(loggerMock, expression, null, null, null);

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the generic ILogger mock.
        /// </summary>
        /// <typeparam name="T">The type of the logger category</typeparam>
        /// <param name="loggerMock">The generic ILogger mock object.</param>
        /// <param name="expression">Expression to verify.</param>
        /// <exception cref="VerifyLogException">
        /// The invocation was not performed on the ILogger mock.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The invocation expression was not defined for one of the logging extensions as defined in the <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions"><see cref="LoggerExtensions"/> class</see> from the <see href="https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/">Microsoft.Extensions.Logging.Abstractions</see> package.
        /// </exception>
        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, Expression<Action<ILogger>> expression)
            => Verify(loggerMock, expression, null, null, null);

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the generic ILogger mock.
        /// </summary>
        /// <typeparam name="T">The type of the logger category</typeparam>
        /// <param name="loggerMock">The generic ILogger mock object.</param>
        /// <param name="expression">Expression to verify.</param>
        /// <param name="failMessage">Message to show if verification fails.</param>
        /// <exception cref="VerifyLogException">
        /// The invocation was not performed on the ILogger mock.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The invocation expression was not defined for one of the logging extensions as defined in the <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions"><see cref="LoggerExtensions"/> class</see> from the <see href="https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/">Microsoft.Extensions.Logging.Abstractions</see> package.
        /// </exception>
        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, Expression<Action<ILogger>> expression, string failMessage)
            => Verify(loggerMock, expression, null, null, failMessage);

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the generic ILogger mock.
        /// </summary>
        /// <typeparam name="T">The type of the logger category</typeparam>
        /// <param name="loggerMock">The generic ILogger mock object.</param>
        /// <param name="expression">Expression to verify.</param>
        /// <param name="times">The number of times a method is expected to be called.</param>
        /// <exception cref="VerifyLogException">
        /// The invocation was not performed on the ILogger mock.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The invocation expression was not defined for one of the logging extensions as defined in the <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions"><see cref="LoggerExtensions"/> class</see> from the <see href="https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/">Microsoft.Extensions.Logging.Abstractions</see> package.
        /// </exception>
        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, Expression<Action<ILogger>> expression, Times times)
            => Verify(loggerMock, expression, times, null, null);

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the generic ILogger mock.
        /// </summary>
        /// <typeparam name="T">The type of the logger category</typeparam>
        /// <param name="loggerMock">The generic ILogger mock object.</param>
        /// <param name="expression">Expression to verify.</param>
        /// <param name="times">The number of times a method is expected to be called.</param>
        /// <param name="failMessage">Message to show if verification fails.</param>
        /// <exception cref="VerifyLogException">
        /// The invocation was not performed on the ILogger mock.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The invocation expression was not defined for one of the logging extensions as defined in the <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions"><see cref="LoggerExtensions"/> class</see> from the <see href="https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/">Microsoft.Extensions.Logging.Abstractions</see> package.
        /// </exception>
        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, Expression<Action<ILogger>> expression, Times times, string failMessage)
            => Verify(loggerMock, expression, times, null, failMessage);


        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the generic ILogger mock.
        /// </summary>
        /// <typeparam name="T">The type of the logger category</typeparam>
        /// <param name="loggerMock">The generic ILogger mock object.</param>
        /// <param name="expression">Expression to verify.</param>
        /// <param name="times">The number of times a method is expected to be called.</param>
        /// <exception cref="VerifyLogException">
        /// The invocation was not performed on the ILogger mock.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The invocation expression was not defined for one of the logging extensions as defined in the <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions"><see cref="LoggerExtensions"/> class</see> from the <see href="https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/">Microsoft.Extensions.Logging.Abstractions</see> package.
        /// </exception>
        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, Expression<Action<ILogger>> expression, Func<Times> times)
            => Verify(loggerMock, expression, null, times, null);

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the generic ILogger mock.
        /// </summary>
        /// <typeparam name="T">The type of the logger category</typeparam>
        /// <param name="loggerMock">The generic ILogger mock object.</param>
        /// <param name="expression">Expression to verify.</param>
        /// <param name="times">The number of times a method is expected to be called.</param>
        /// <param name="failMessage">Message to show if verification fails.</param>
        /// <exception cref="VerifyLogException">
        /// The invocation was not performed on the ILogger mock.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The invocation expression was not defined for one of the logging extensions as defined in the <see href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions"><see cref="LoggerExtensions"/> class</see> from the <see href="https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/">Microsoft.Extensions.Logging.Abstractions</see> package.
        /// </exception>
        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, Expression<Action<ILogger>> expression, Func<Times> times, string failMessage)
            => Verify(loggerMock, expression, null, times, failMessage);
        #endregion

        private static void Verify<T>(Mock<T> loggerMock, Expression<Action<ILogger>> expression, Times? times, Func<Times> timesFunc,
            string failMessage) where T : class
        {
            GuardVerifyExpressionIsForLoggerExtensions(expression);

            var verifyLogExpression = VerifyLogExpression.From(expression);
            var verifyExpression = CreateMoqVerifyExpressionFrom<T>(verifyLogExpression);
            try
            {
                if (timesFunc != null)
                {
                    loggerMock.Verify(verifyExpression, timesFunc, failMessage);
                }
                else if (times.HasValue)
                {
                    loggerMock.Verify(verifyExpression, times.Value, failMessage);
                }

                loggerMock.Verify(verifyExpression, failMessage);
            }
            catch (MockException ex)
            {
                throw new VerifyLogException(BuildExceptionMessage(ex, expression), ex);
            }
        }

        private static Expression<Action<T>> CreateMoqVerifyExpressionFrom<T>(VerifyLogExpression verifyLogExpression)
        {
            var logLevelExpression = CreateLogLevelExpression(verifyLogExpression);
            var eventIdExpression = CreateEventIdExpression(verifyLogExpression);
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
            => Expression.Convert(BuildItIsAnyExpression<object>(), typeof(Func<It.IsAnyType, Exception, string>));

        private static Expression CreateEventIdExpression(VerifyLogExpression verifyLogExpression) 
            => verifyLogExpression.EventIdExpression ?? BuildItIsAnyExpression<EventId>();

        private static Expression CreateExceptionExpression(VerifyLogExpression verifyLogExpression)
        {
            if (verifyLogExpression.ExceptionExpression == null)
            {
                return BuildItIsAnyExpression<Exception>();
            }

            var exception = verifyLogExpression.Args.Exception;
            if (exception == null)
            {
                return verifyLogExpression.ExceptionExpression;
            }

            // an Expression arg is given, create an It.Is<Exception>(e => Compare(e, arg))
            // build It.Is(CompareExceptions(exception)),
            var exceptionParam = Expression.Parameter(typeof(Exception));
            var exceptionConstantExpression = Expression.Constant(exception, typeof(Exception));
            var compareExceptionsCallExpression = Expression.Call(typeof(VerifyLogExtensions), nameof(CompareExceptions), null, exceptionConstantExpression, exceptionParam);
            var compareExpression = Expression.Lambda<Func<Exception, bool>>(compareExceptionsCallExpression, exceptionParam);
            var compareExceptionQuoteExpression = Expression.Quote(compareExpression);
            var itIsExceptionExpression = Expression.Call(typeof(It), "Is", new[] { typeof(Exception) }, compareExceptionQuoteExpression);
            return itIsExceptionExpression;
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
                        var verifyLogConstantExpression = Expression.Constant(verifyLogExpression, typeof(VerifyLogExpression));
                        var compareMessagesCallExpression = Expression.Call(typeof(VerifyLogExtensions), nameof(CompareMessages), null, messageConstantExpression, verifyLogConstantExpression, vParam);
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
            var itIsMessageExpression = Expression.Call(typeof(It), "Is", new[] { typeof(It.IsAnyType) }, compareMessageQuoteExpression);
            return itIsMessageExpression;

            Expression<Func<object, Type, bool>> CreateCompareLambdaFrom(Expression methodCallExpression)
            {
                var vParamToStringExpression = Expression.Call(vParam, typeof(object).GetMethod(nameof(ToString))!);
                var verifyLogConstantExpression = Expression.Constant(verifyLogExpression, typeof(VerifyLogExpression));

                var compareMessagesCallExpression = Expression.Call(typeof(VerifyLogExtensions), nameof(CompareMessages), null,
                    vParamToStringExpression, verifyLogConstantExpression, methodCallExpression);
                compareExpression = Expression.Lambda<Func<object, Type, bool>>(compareMessagesCallExpression, vParam, tParam);
                return compareExpression;
            }

            MethodCallExpression CreatevParamToStringExpression()
            {
                var vParamToStringExpression = Expression.Call(vParam, typeof(object).GetMethod(nameof(ToString))!);
                return vParamToStringExpression;
            }
        }

        private static Expression BuildItIsAnyExpression<T>()
            => Expression.Call(typeof(It), "IsAny", new[] { typeof(T) });

        private static void GuardVerifyExpressionIsForLoggerExtensions(Expression expression)
        {
            ////https://github.com/dotnet/runtime/blob/e3ffd343ad5bd3a999cb9515f59e6e7a777b2c34/src/libraries/Microsoft.Extensions.Logging.Abstractions/src/LoggerExtensions.cs
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
                var message = $"{nameof(Moq)}.{nameof(ILogger)} supports only specific Logging extensions " +
                              "as defined in the <a href=\"https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions\">LoggerExtensions class</a> " +
                              "form the <a href=\"https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions/\">\"Microsoft.Extensions.Logging.Abstractions\"</a> package. " +
                              $"The Log use case must be on of ({string.Join(", ", supportedMethods)}). ";

                if (!string.IsNullOrEmpty(methodName))
                {
                    message += $"{Environment.NewLine}" +
                               $"{Environment.NewLine}" +
                               $"The resolved method `{methodName}` in the verify expression is not one of these.";
                }
                else
                {
                    message += $"{Environment.NewLine}" +
                               $"{Environment.NewLine}" +
                               "A method name could not be resolved from the verify expression.";
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

        private static bool CompareMessages(string expectedMessageFormat, VerifyLogExpression verifyLogExpression, object actualMessageValues)
        {
            var actualMessageFormatted = actualMessageValues.ToString();

            if (expectedMessageFormat == null && actualMessageFormatted == NullMessageFormatted)
            {
                return true;
            }

            if (expectedMessageFormat == null || actualMessageFormatted == NullMessageFormatted)
            {
                return false;
            }

            if (!(actualMessageValues is IReadOnlyList<KeyValuePair<string, object>>))
            {
                return actualMessageFormatted.IsWildcardMatch(expectedMessageFormat);
            }

            var (actualMessageFormat, actualMessageArgs) = ExtractActualMessageFormatAndArgs(actualMessageValues);
            var (expectedMessageFormattedWithSuccess, expectedMessageFormatted) = TryFormatLogValues(expectedMessageFormat, verifyLogExpression.Args.MessageArgs);

            if (expectedMessageFormat.IsWildcard())
            {
                if (verifyLogExpression.HasExpectedMessageArgs)
                {
                    return actualMessageFormat.IsWildcardMatch(expectedMessageFormat) && MessageArgsMatch(verifyLogExpression.MessageArgsExpression, actualMessageArgs);
                }

                if (actualMessageFormat.IsWildcardMatch(expectedMessageFormat)
                    || expectedMessageFormattedWithSuccess && actualMessageFormatted.IsWildcardMatch(expectedMessageFormatted))
                {
                    return true;
                }
            }
            else
            {
                if (verifyLogExpression.HasExpectedMessageArgs)
                {
                    return actualMessageFormat.EqualsIgnoreCase(expectedMessageFormat) && MessageArgsMatch(verifyLogExpression.MessageArgsExpression, actualMessageArgs);
                }

                if (actualMessageFormat.EqualsIgnoreCase(expectedMessageFormat)
                    || expectedMessageFormattedWithSuccess && actualMessageFormatted.EqualsIgnoreCase(expectedMessageFormatted))
                {
                    return true;
                }
            }

            return false;
        }

        private static (string fromat, object[] args) ExtractActualMessageFormatAndArgs(object actualMessageValues)
        {
            var actualMessage = (IReadOnlyList<KeyValuePair<string, object>>)actualMessageValues;
            var actualMessageFormat = actualMessage.First(c => c.Key == OriginalFormat).Value.ToString();
            var actualMessageArgs = actualMessage.Where(c => c.Key != OriginalFormat).Select(c => c.Value).ToArray();
            return (actualMessageFormat, actualMessageArgs);
        }

        private static (bool success, string formatted) TryFormatLogValues(string format, object[] arguments)
        {
            try
            {
                return (true, FormatLogValues(format, arguments));
            }
            catch (FormatException)
            {
                return (false, null);
            }
        }

        private static string FormatLogValues(string format, object[] arguments)
        {
            //https://github.com/dotnet/runtime/blob/e3ffd343ad5bd3a999cb9515f59e6e7a777b2c34/src/libraries/Microsoft.Extensions.Logging.Abstractions/src/FormattedLogValues.cs#L30
            var formattedLogValuesType = typeof(LoggerExtensions).Assembly.GetTypes()
                .First(c => c.Name == "FormattedLogValues");

            var formattedLogValues = Activator.CreateInstance(formattedLogValuesType,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, new object[] { format, arguments },
                null);
            return formattedLogValues.ToString();
        }

        private static bool MessageArgsMatch(Expression expectedMessageArgsExpression, object[] actualArgs)
        {
            // see this Test for reference https://github.com/moq/moq4/blob/61f420f3d44527ce652883ce857fc8b3bdabafca/tests/Moq.Tests/Matchers/ParamArrayMatcherFixture.cs#L19

            // create Expression<> _ = x => x.Method(expectedMessageArgsExpression);
            //

            var xParameter = Expression.Parameter(typeof(IX), "x");
            var instance = Expression.Constant(new X(), typeof(IX));
            var methodCallExpression = Expression.Call(instance, typeof(IX).GetMethod("Method")!, expectedMessageArgsExpression);
            var methodCallLambda = Expression.Lambda(methodCallExpression, xParameter);
            var parameter = typeof(IX).GetMethod("Method")!.GetParameters().Single();

            // Execute the following Moq code
            //          var (matcher, _) = MatcherFactory.CreateMatcher(expr, parameter);
            //          matcher.Matches(actualArgs, typeof(object[]))
            //

            var matcherFactoryType = typeof(Mock).Assembly.GetTypes().First(c => c.Name == "MatcherFactory");
            var createMatcherMethod =
                matcherFactoryType.GetMethod("CreateMatcher", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(Expression), typeof(ParameterInfo) }, null);

            // result is typeof(Pair<IMatcher, Expression>); Pair is a type from Moq, having two fields Item1, Item2
            // we need Item1
            var matcherResult = createMatcherMethod!.Invoke(null, new object[] { ((MethodCallExpression)methodCallLambda.Body).Arguments.Single(), parameter });
            var resultType = matcherResult.GetType();
            var matcherField = resultType.GetField("Item1");
            var matcher = matcherField.GetValue(matcherResult);
            var matcherType = matcher.GetType();

            // invoke matcher.Matches(actualArgs, typeof(object[]))
            var matchesMethod =
                matcherType.GetMethod("Matches", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(object[]), typeof(Type) }, null);
            var isMatch = (bool)matchesMethod!.Invoke(matcher, new object[] { actualArgs, typeof(object[]) });
            return isMatch;
        }

        private static string BuildExceptionMessage(MockException ex, Expression expression)
        {
            var stringBuilderExtensionsType = typeof(Mock).Assembly.GetTypes().First(c => c.Name == "StringBuilderExtensions");
            var appendExpressionMethod =
                stringBuilderExtensionsType.GetMethod("AppendExpression", BindingFlags.Static | BindingFlags.Public);
            var stringBuilder = new StringBuilder();
            appendExpressionMethod!.Invoke(null, new object[] { stringBuilder, expression });
            var expressionText = stringBuilder.ToString();

            var moqIndications = ex.Message.Split(':')[0];

            return $"{moqIndications}: {expressionText}" +
                   $"{Environment.NewLine}" +
                   $"{Environment.NewLine}";
        }
    }

    internal interface IX { void Method(params object[] args); }
    internal class X : IX { public void Method(params object[] args) { throw new NotSupportedException("This method should never be called, only inspected."); } }
}
