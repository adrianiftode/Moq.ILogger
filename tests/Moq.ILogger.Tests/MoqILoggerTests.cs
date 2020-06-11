using Microsoft.Extensions.Logging;
using System;
using Xunit;
using FluentAssertions;

namespace Moq.Tests
{
    public interface IInterface
    { 
    }
    public class MoqILoggerTests
    {
        [Fact]
        public void Verify_log_information_with_an_expected_message_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.Verify(LogLevel.Information, "Test message");

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_log_level_was_called_but_it_wasnt_then_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogDebug("Test message");

            Action act = () => loggerMock.Verify(LogLevel.Information, "Test message");

            act.Should().ThrowExactly<ILoggerMockException>()
                .WithMessage("*.LogInformation(\"Test message\")*");
        }

        [Fact]
        public void Verify_a_message_that_is_not_a_match_then_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.Verify(LogLevel.Information, "A different message");

            act.Should().ThrowExactly<ILoggerMockException>()
                .WithMessage("*.LogInformation(\"A different message\")*");
        }

        [Fact]
        public void Verify_with_a_message_that_differs_by_case_then_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test Message");

            Action act = () => loggerMock.Verify(LogLevel.Information, "Test message");

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_formatted_log_message_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message {0}", 1);

            Action act = () => loggerMock.Verify(LogLevel.Information, "Test message 1");

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_formatted_log_message_with_different_parameter_value_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message {0}", 1);

            Action act = () => loggerMock.Verify(LogLevel.Information, "Test message 2");

            act.Should().ThrowExactly<ILoggerMockException>()
                .WithMessage("*.LogInformation(\"Test message 2\")*");
        }

        [Fact]
        public void Verify_a_message_against_wildcard_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test gibberish message");

            Action act = () => loggerMock.Verify(LogLevel.Information, "Test*message");

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_message_that_is_part_of_the_log_message_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message with more content");

            Action act = () => loggerMock.Verify(LogLevel.Information, "Test message");

            act.Should().ThrowExactly<ILoggerMockException>()
                .WithMessage("*.LogInformation(\"Test message\")*");
        }

        [Fact]
        public void Verify_a_structured_message_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.Verify(LogLevel.Information, "Processed { Latitude = 25, Longitude = 134 } in 034 ms.");

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_structured_message_using_wildcards_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.Verify(LogLevel.Information, "Processed * in * ms.");

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_an_error_is_logged_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogWarning(new Exception("Something unexpected happened, but will survive."), "Test message");

            Action act = () => loggerMock.Verify(LogLevel.Warning, new Exception("Something unexpected happened, but will survive."), "Test message");

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_an_error_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogWarning(new Exception("Some error message."), "Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogWarning(It.Is<Exception>(e => e.Message == "Some error message."), "Test message"));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_an_error_with_different_messages_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogWarning(new Exception("Some error message."), "Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogWarning(It.Is<Exception>(e => e.Message == "Some different message."), "Test message"));

            act.Should().ThrowExactly<ILoggerMockException>()
                .WithMessage("*.LogWarning(\"Test message\")*Some different message*");
        }

        [Fact]
        public void Verify_an_error_against_a_counterpart_it_throws_because_it_cannot_compare_the_exceptions()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogWarning(new Exception("Some error message."), "Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogWarning(new Exception("Some error message."), "Test message"));

            act.Should().ThrowExactly<ILoggerMockException>()
                .WithMessage("*.LogWarning(\"Test message\")*Some error message*");
        }

        [Fact]
        public void Verify_any_error_any_message_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogWarning(new Exception("Some error message."), "Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogWarning(It.IsAny<Exception>(), It.IsAny<string>()));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_any_message_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogInformation(It.IsAny<string>()));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_message_with_matching_criteria_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogInformation(It.Is<string>(msg => msg.Contains("Test"))));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_message_with_no_matching_criteria_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogInformation(It.Is<string>(msg => msg.Contains("Expecting something else"))));

            act.Should().ThrowExactly<ILoggerMockException>()
                .WithMessage("*.LogInformation(\"\")*Expecting something else*");
        }

        [Fact]
        public void Verify_null_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogWarning(new Exception("Some error message."), null);

            Action act = () => loggerMock.VerifyLog(c => c.LogWarning(It.IsAny<Exception>(), null));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_null_against_log_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogWarning(new Exception("Some error message."), "Some message");

            Action act = () => loggerMock.VerifyLog(c => c.LogWarning(It.IsAny<Exception>(), null));

            act.Should().ThrowExactly<ILoggerMockException>()
               .WithMessage("*.LogWarning(\"\")*");
        }

        [Fact]
        public void Verify_it_is_not_null_message_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogInformation(It.IsNotNull<string>()));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_it_is_not_null_when_message_is_null_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation(null);

            Action act = () => loggerMock.VerifyLog(c => c.LogInformation(It.IsNotNull<string>()));

            act.Should().ThrowExactly<ILoggerMockException>()
               .WithMessage("*.LogInformation*");
        }

        [Fact]
        public void Verify_it_is_regex_message_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogInformation(It.IsRegex("^(.*)$")));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_it_is_regex_when_message_does_not_match_the_regex_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogInformation(It.IsRegex("[0-9]")));

            act.Should().ThrowExactly<ILoggerMockException>()
             .WithMessage("*.LogInformation*");
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Critical)]
        public void Verify_log_level_it_verifies(LogLevel logLevel)
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.Log(logLevel, default, "Some message", null, null);

            Action act = () => loggerMock.Verify(logLevel, "Some message");

            act.Should().NotThrow<ILoggerMockException>();
        }
    }
}
