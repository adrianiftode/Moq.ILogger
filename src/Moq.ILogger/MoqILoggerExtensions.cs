using Microsoft.Extensions.Logging;
using Moq.Internal;
using System;

namespace Moq
{
    public static class MoqILoggerExtensions
    {
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

        private static bool CompareMessages(string message, object v)
            => v.ToString().IsWildcardMatch(message);
    }
}
