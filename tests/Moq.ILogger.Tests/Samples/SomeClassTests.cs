using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;
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

        public void LoggingError(string name)
            => _logger.LogError(new ArgumentException("The given name is not ok", nameof(name)), $"This operation failed for name {name}");

        public void SemanticLogging()
        {
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;

            _logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);
        }

        public void MultipleLoggingCalls()
        {
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;

            _logger.LogInformation("Processed {@Position} in {Elapsed:000} ms with success.", position, elapsedMs);
            _logger.LogInformation("Processed {@Position} in {Elapsed:000} ms with failure.", position, elapsedMs);
        }

        public void LoggingWithCustomFormat()
        {
          var position = new { Latitude = 25, Longitude = 134 };
          var elapsedMs = 34;

          _logger.LogInformation(GetMessage(), position, elapsedMs);
        }

        public static string GetMessage()
            => "Processed {@Position} in {Elapsed:000} ms.";

        public static string GetOtherMessage()
          => "Not Processed {@Position} in {Elapsed:000} ms.";
  }

    public class SomeClassTests
    {
        [Fact]
        public void Verify_log_information_with_a_message()
        {
            var loggerMock = new Mock<ILogger<SomeClass>>();
            var sut = new SomeClass(loggerMock.Object);

            sut.LoggingInformation();

            loggerMock.VerifyLog(logger => logger.LogInformation("This operation is successful."));
            loggerMock.VerifyLog(logger => logger.LogInformation((EventId)0, "This operation is successful."));
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
            loggerMock.VerifyLog(logger => logger.LogWarning((EventId)0, It.IsAny<ArgumentException>(), "*failed*"));
            loggerMock.VerifyLog(logger => logger.LogWarning(It.IsAny<EventId>(), It.IsAny<ArgumentException>(), "*failed*"));
            // ReSharper disable once NotResolvedInText
            loggerMock.VerifyLog(logger => logger.LogWarning(It.IsAny<EventId>(), new ArgumentException("The given name is not ok", "name"), "*failed*"));
        }

        [Fact]
        public void Verify_exceptions_logged_as_errors()
        {
            var name = "Adrian";
            var loggerMock = new Mock<ILogger<SomeClass>>();
            var sut = new SomeClass(loggerMock.Object);

            sut.LoggingError(name);

            loggerMock.VerifyLog(logger => logger.LogError(It.Is<ArgumentException>(ex => ex.ParamName == "name"), $"*{name}*"));
        }

        [Fact]
        public void Verify_semantic_logging()
        {
            var loggerMock = new Mock<ILogger<SomeClass>>();
            var sut = new SomeClass(loggerMock.Object);

            sut.SemanticLogging();


            loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", It.Is<object[]>(arg => (int)arg[1] == 34 )));

            loggerMock.VerifyLog(logger => logger.LogInformation("Processed { Latitude = 25, Longitude = 134 } in 034 ms."));

            loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", new { Latitude = 25, Longitude = 134 }, 34));
            loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", It.IsAny<It.IsAnyType>(), It.IsAny<int>()));
            loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", It.Is<object[]>(arg => arg != null)));

            loggerMock.VerifyLog(logger => logger.LogInformation("Processed { Latitude = *, Longitude = * } in * ms."));
            loggerMock.VerifyLog(logger => logger.LogInformation("Processed * in * ms."));
            loggerMock.VerifyLog(logger => logger.LogInformation("Processed*{@Position}*{Elapsed:000}*ms."));

            loggerMock.VerifyLog(logger => logger.LogInformation("Processed * in * ms.", It.IsAny<It.IsAnyType>(), It.IsAny<int>()));
            loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position}*{Elapsed:000} ms.", new { Latitude = 25, Longitude = 134 }, 34));
        }

        [Fact]
        public void Verify_multiple_logging_calls()
        {
            var loggerMock = new Mock<ILogger<SomeClass>>();
            var sut = new SomeClass(loggerMock.Object);

            sut.MultipleLoggingCalls();

            loggerMock.VerifyLog(logger => logger.LogInformation("Processed { Latitude = 25, Longitude = 134 } in 034 ms with failure."));
            loggerMock.VerifyLog(logger => logger.LogInformation("Processed { Latitude = 25, Longitude = 134 } in 034 ms with*"), Times.Exactly(2));
        }

        [Fact]
        public void Verify_Logging_WithCustomFormat()
        {
          var loggerMock = new Mock<ILogger<SomeClass>>();
          var sut = new SomeClass(loggerMock.Object);

          sut.LoggingWithCustomFormat();

          loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", new { Latitude = 25, Longitude = 134 }, 34));
          loggerMock.VerifyLog(logger => logger.LogInformation(SomeClass.GetMessage(), new { Latitude = 25, Longitude = 134 }, 34));
          loggerMock.VerifyLog(logger => logger.LogInformation(SomeClass.GetOtherMessage(), new { Latitude = 25, Longitude = 134 }, 34));
        }

        [Fact]
        public void Test()
        {
          var serviceMock = new Mock<IService>();
          var sut = new SomeOtherClass(serviceMock.Object);

          sut.GetMessage();

          Expression<Action<IService>> expression = logger => logger.GetMessage("Processed {@Position} in {Elapsed:000} ms.", 1);
          serviceMock.Verify(expression);
          serviceMock.Verify(logger => logger.GetMessage(SomeClass.GetMessage(), 1));
          serviceMock.Verify(logger => logger.GetMessage(SomeClass.GetMessage(), It.IsAny<int>()));
          serviceMock.Verify(logger => logger.GetMessage(SomeClass.GetOtherMessage(), It.IsAny<int>()));
        }

        public interface IService
        {
            string GetMessage(string argument, int arg2);
        }

        public class SomeOtherClass
        {
          private readonly IService _service;

          public SomeOtherClass(IService service)
          {
            _service = service;
          }

          public string GetMessage() => _service.GetMessage(SomeClass.GetMessage(), 1);
        }
    }
}