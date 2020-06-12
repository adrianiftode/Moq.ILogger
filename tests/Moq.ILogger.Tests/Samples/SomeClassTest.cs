using Microsoft.Extensions.Logging;
using System;
using Xunit;

namespace Moq.Tests.Samples
{
    public class SomeClass
    {
        private readonly ILogger<SomeClass> _logger;
        public SomeClass(ILogger<SomeClass> logger) => _logger = logger;

        public void LoggingInformation()
            => _logger.LogInformation("This operation is successfull.");
        public void LoggingWarning(string name)
            => _logger.LogWarning(new ArgumentException("The given name is not ok", nameof(name)), "This operation failed, but let's log an warning only");
    }

    public class SomeClassTest
    {
        [Fact]
        public void Verify_log_information_with_a_message()
        {
            var loggerMock = new Mock<ILogger<SomeClass>>();
            var sut = new SomeClass(loggerMock.Object);

            sut.LoggingInformation();

            loggerMock.VerifyLog(logger => logger.LogInformation("This operation is successfull."));
            loggerMock.VerifyLog(logger => logger.LogInformation("This * is successfull."));
            loggerMock.VerifyLog(logger => logger.LogInformation(It.Is<string>(msg => msg.Length > 5)));
            loggerMock.VerifyLog(logger => logger.LogInformation(It.IsAny<string>()));
            loggerMock.VerifyLog(logger => logger.LogInformation(It.IsNotNull<string>()));
            loggerMock.VerifyLog(logger => logger.LogInformation(It.IsRegex(".*")));
        }

        [Fact]
        public void Verify_errors()
        {
            var loggerMock = new Mock<ILogger<SomeClass>>();
            var sut = new SomeClass(loggerMock.Object);

            sut.LoggingWarning(null);

            loggerMock.VerifyLog(logger => logger.LogWarning(It.IsAny<ArgumentException>(), It.IsAny<string>()));
            loggerMock.VerifyLog(logger => logger.LogWarning(It.Is<ArgumentException>(ex => ex.ParamName == "name"), "*failed*"));
            loggerMock.VerifyLog(logger => logger.LogWarning((EventId)10, It.IsAny<ArgumentException>(), "*failed*"));
            loggerMock.VerifyLog(logger => logger.LogWarning(It.IsAny<EventId>(), It.IsAny<ArgumentException>(), "*failed*"));
            loggerMock.VerifyLog(logger => logger.LogWarning(It.IsAny<EventId>(), new ArgumentException("The given name is not ok", "name"), "*failed*"));
        }
    }
}
