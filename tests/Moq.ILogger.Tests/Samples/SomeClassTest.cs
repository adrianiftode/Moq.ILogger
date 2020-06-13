using Microsoft.Extensions.Logging;
using System;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Moq.Tests.Samples
{
    public class SomeClass
    {
        private readonly ILogger<SomeClass> _logger;
        public SomeClass(ILogger<SomeClass> logger) => _logger = logger;

        public void LoggingInformation()
            => _logger.LogInformation("This operation is successful.");

        public void LoggingWarning(string name)
            => _logger.LogWarning(new ArgumentException("The given name is not ok", nameof(name)), "This operation failed, but let's log an warning only");

        public void SemanticLogging()
        {
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;

            _logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);
        }
    }

    public class SomeClassTest
    {
        [Fact]
        public void Verify_log_information_with_a_message()
        {
            var loggerMock = new Mock<ILogger<SomeClass>>();
            var sut = new SomeClass(loggerMock.Object);

            sut.LoggingInformation();

            loggerMock.VerifyLog(logger => logger.LogInformation("This operation is successful."));
            loggerMock.VerifyLog(logger => logger.LogInformation("This * is successful."));
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
            // ReSharper disable once RedundantCast
            loggerMock.VerifyLog(logger => logger.LogWarning((EventId)10, It.IsAny<ArgumentException>(), "*failed*"));
            loggerMock.VerifyLog(logger => logger.LogWarning(It.IsAny<EventId>(), It.IsAny<ArgumentException>(), "*failed*"));
            // ReSharper disable once NotResolvedInText
            loggerMock.VerifyLog(logger => logger.LogWarning(It.IsAny<EventId>(), new ArgumentException("The given name is not ok", "name"), "*failed*"));
        }

        [Fact]
        public void Semantic_Logging()
        {
            var loggerMock = new Mock<ILogger<SomeClass>>();
            var sut = new SomeClass(loggerMock.Object);

            sut.SemanticLogging();

            loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", new { Latitude = 25, Longitude = 134 }, 34));
            loggerMock.VerifyLog(logger => logger.LogInformation("Processed * in * ms."));
            loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} * {Elapsed:000} ms.", new { Latitude = 25, Longitude = 134 }, 34));
            //TODO  add support for It.Is for parameters
            //loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} * {Elapsed:000} ms.", It.IsAny<It.IsAnyType>(), It.Is<int>(ms => ms > 0)));
            //TODO  wildcard probably needs to be reanalyzed, if it should be used or not
            //loggerMock.VerifyLog(logger => logger.LogInformation("*{@Position}*{Elapsed:000}*"));
        }
    }
}