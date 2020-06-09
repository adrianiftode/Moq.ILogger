using Microsoft.Extensions.Logging;
using System;
using Xunit;
using FluentAssertions;

namespace Moq.ILogger.Tests
{
    public interface IInterface
    { 
    }
    public class MoqILoggerTests
    {
        [Fact]
        public void Verify_log_information_with_an_expected_message_it_verifies()
        {
            var iloggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
            iloggerMock.Object.LogInformation("Test message");

            Action act = () => iloggerMock.Verify(LogLevel.Information, "Test message");

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_log_level_was_called_but_it_wasnt_then_it_throws()
        {
            var iloggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
            iloggerMock.Object.LogDebug("Test message");

            Action act = () => iloggerMock.Verify(LogLevel.Information, "Test message");

            act.Should().ThrowExactly<ILoggerMockException>()
                .WithMessage("*.LogInformation(\"Test message\")*");
        }

        [Fact]
        public void Verify_a_message_that_is_not_a_match_then_it_throws()
        {
            var iloggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
            iloggerMock.Object.LogInformation("Test message");

            Action act = () => iloggerMock.Verify(LogLevel.Information, "A different message");

            act.Should().ThrowExactly<ILoggerMockException>()
                .WithMessage("*.LogInformation(\"A different message\")*");
        }

        [Fact]
        public void Verify_with_a_message_that_differs_by_case_then_it_verifies()
        {
            var iloggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
            iloggerMock.Object.LogInformation("Test Message");

            Action act = () => iloggerMock.Verify(LogLevel.Information, "Test message");

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_formatted_log_message_it_verifies()
        {
            var iloggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
            iloggerMock.Object.LogInformation("Test message {0}", 1);

            Action act = () => iloggerMock.Verify(LogLevel.Information, "Test message 1");

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_formatted_log_message_with_different_parameter_value_it_throws()
        {
            var iloggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
            iloggerMock.Object.LogInformation("Test message {0}", 1);

            Action act = () => iloggerMock.Verify(LogLevel.Information, "Test message 2");

            act.Should().ThrowExactly<ILoggerMockException>()
                .WithMessage("*.LogInformation(\"Test message 2\")*");
        }

        [Fact]
        public void Verify_a_message_against_wildcard_it_verifies()
        {
            var iloggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
            iloggerMock.Object.LogInformation("Test gibberish message");

            Action act = () => iloggerMock.Verify(LogLevel.Information, "Test*message");

            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Critical)]
        public void Verify_log_level_it_verifies(LogLevel logLevel)
        {
            var iloggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
            iloggerMock.Object.Log(logLevel, default, "Some message", null, null);

            Action act = () => iloggerMock.Verify(logLevel, "Some message");

            act.Should().NotThrow<ILoggerMockException>();
        }
    }
}
