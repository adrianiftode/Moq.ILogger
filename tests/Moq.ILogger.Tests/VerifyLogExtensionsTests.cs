using FluentAssertions;
using Microsoft.Extensions.Logging;
using System;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Moq.Tests
{
    public class VerifyLogExtensionsTests
    {
        [Fact]
        public void Verify_log_debug_with_an_expected_message_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogDebug("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogDebug("Test message"));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_log_information_with_an_expected_message_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Test message"));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_log_warning_with_an_expected_message_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogWarning("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogWarning("Test message"));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_log_error_with_an_expected_message_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogError("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogError("Test message"));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_log_critical_with_an_expected_message_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogCritical("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogCritical("Test message"));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_log_level_was_called_but_it_was_not_then_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogDebug("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Test message"));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation(\"Test message\"*");
        }

        [Fact]
        public void Verify_a_message_that_is_not_a_match_then_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("A different message"));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation(\"A different message\"*");
        }

        [Fact]
        public void Verify_with_a_message_that_differs_by_case_then_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test Message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Test message"));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_formatted_log_message_and_the_param_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message {0}", 1);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Test message {0}", 1));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_formatted_log_message_and_with_different_param_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message {0}", 1);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Test message {0}", 2));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation(\"Test message {0}\"*2*");
        }

        [Fact]
        public void Verify_a_formatted_log_message_with_different_parameter_value_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message {0}", 1);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Test message 2"));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation(\"Test message 2\"*");
        }

        [Fact]
        public void Verify_a_message_against_wildcard_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test gibberish message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Test*message"));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_message_with_an_wildcard_but_different_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test gibberish message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Test*something"));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation(\"Test*something\"*");
        }

        [Fact]
        public void Verify_a_multiline_message_against_wildcard_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation(@"The configuration doesn't define any stages to run.
            Please make sure that at least one stage is set to run.");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("*configuration*"));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_message_that_is_part_of_the_log_message_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message with more content");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Test message"));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation(\"Test message\"*");
        }

        [Fact]
        public void Verify_a_structured_message_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", new { Latitude = 25, Longitude = 134 }, 34));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_structured_message_when_expected_is_formatted_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Processed { Latitude = 25, Longitude = 134 } in 034 ms."));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_structured_message_when_expected_is_formatted_but_has_a_parameter_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Processed { Latitude = 25, Longitude = 134 } in 034 ms.", new { Latitude = 25, Longitude = 134 }));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation*");
        }

        [Fact]
        public void Verify_a_structured_message_using_wildcards_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Processed * in * ms."));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_structured_message_using_wildcards_and_containing_parameters_names_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Processed*{@Position}*{Elapsed:000}*ms."));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_structured_message_using_wildcards_and_containing_formatted_message_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Processed { Latitude = *, Longitude = * } in * ms."));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_structured_message_using_wildcards_and_parameters_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Processed * in * ms.", new { Latitude = 25, Longitude = 134 }, 34));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_structured_message_using_wildcards_with_parameters_names_and_parameters_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} * {Elapsed:000} ms.", new { Latitude = 25, Longitude = 134 }, 34));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_structured_message_using_wildcards_with_parameters_names_and_parameters_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Processed { Latitude = *, Longitude = * } in * ms.", new { Latitude = 25, Longitude = 134 }, 34));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation*");
        }

        [Fact]
        public void Verify_a_message_using_wildcards_with_interpolated_message_it_verifies()
        {
            var loggerMock = new Mock<ILogger<object>>();
            var interpolatedMessageArg = "Arg";
            loggerMock.Object.LogInformation($"Test message {interpolatedMessageArg}");

            Action act = () => loggerMock.VerifyLog(c => c.LogInformation($"*{interpolatedMessageArg}*"));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_message_using_wildcards_with_interpolated_message_it_throws()
        {
            var loggerMock = new Mock<ILogger<object>>();
            var interpolatedMessageArg1 = "Arg 1";
            var interpolatedMessageArg2 = "Arg 2";
            loggerMock.Object.LogInformation($"Test message {interpolatedMessageArg1}");

            Action act = () => loggerMock.VerifyLog(c => c.LogInformation($"*{interpolatedMessageArg2}*"));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation*");
        }

        [Fact]
        public void Verify_a_structured_message_when_parameters_differ_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 0, Longitude = 0 };
            var elapsedMs = 0;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", new { Latitude = 25, Longitude = 134 }, 34));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation*");
        }

        [Fact]
        public void Verify_a_structured_message_with_fewer_parameters_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 0, Longitude = 0 };
            var elapsedMs = 0;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", new { Latitude = 25, Longitude = 134 }));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation*");
        }

        [Fact]
        public void Verify_a_structured_message_with_more_parameters_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 0, Longitude = 0 };
            var elapsedMs = 0;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", new { Latitude = 25, Longitude = 134 }, 34, true));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation*");
        }

        [Fact]
        public void Verify_a_structured_message_with_matching_It_Is_parameters_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", It.IsAny<It.IsAnyType>(), It.Is<int>(ms => ms == 34)));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_a_structured_message_with_not_matching_It_Is_parameters_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", null, It.Is<int>(ms => ms != 34)));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation*");
        }

        [Fact]
        public void Verify_when_args_array_is_matched_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.",
                It.IsAny<object[]>()));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_when_args_array_is_not_matched_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.",
                It.Is<object[]>(o => o == null)));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*Expected invocation on the mock at least once*(o => o == null)*");
        }

        [Fact]
        public void Verify_a_message_with_method_call_it_verifies()
        {
            var loggerMock = new Mock<ILogger<object>>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogInformation(GetMessage()));

            act.Should().NotThrow();
        }
        string GetMessage() => "Test message";

        [Fact]
        public void Verify_a_message_with_method_call_that_returns_a_format_it_verifies()
        {
            var loggerMock = new Mock<ILogger<object>>();
            loggerMock.Object.LogInformation(GetMessageWithFormat(), 0);

            Action act = () => loggerMock.VerifyLog(c => c.LogInformation(GetMessageWithFormat(), It.IsAny<int>()));

            act.Should().NotThrow();
        }
        string GetMessageWithFormat() => "Test message {Id}";

        [Fact]
        public void Verify_a_message_with_a_different_one_from_a_method_call_it_throws()
        {
            var loggerMock = new Mock<ILogger<object>>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogInformation(GetNotAMessage()));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation(VerifyLogExtensionsTests.GetNotAMessage(), new[] {  })*")
                .Which.InnerException!.Message.Should().Match("*logger => logger.Log<It.IsAnyType>(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => VerifyLogExtensions.CompareMessages(\"Not a test message\", VerifyLogExpression, v.ToString())), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>())*");
        }
        string GetNotAMessage() => "Not a test message";

        [Fact]
        public void Verify_a_wildcard_message_with_method_call_it_verifies()
        {
            var loggerMock = new Mock<ILogger<object>>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogInformation(GetWildcardMessage()));

            act.Should().NotThrow();
        }
        string GetWildcardMessage() => "*message*";

        [Fact]
        public void Verify_a_wildcard_message_with_a_different_one_from_a_method_call_it_throws()
        {
            var loggerMock = new Mock<ILogger<object>>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogInformation(GetWildcardNotAMessage()));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation(VerifyLogExtensionsTests.GetWildcardNotAMessage(), new[] {  })*")
                .Which.InnerException!.Message.Should().Match("*logger => logger.Log<It.IsAnyType>(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => VerifyLogExtensions.CompareMessages(\"*no match*\", VerifyLogExpression, v.ToString())), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>())*");
        }
        string GetWildcardNotAMessage() => "*no match*";

        [Fact]
        public void Verify_a_message_one_constructed_call_it_throws()
        {
            var loggerMock = new Mock<ILogger<object>>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogInformation(new string(new[] { 'a' })));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation(new string(new[] { a }), new[] {  })*")
                .Which.InnerException!.Message.Should().Match("*logger => logger.Log<It.IsAnyType>(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => VerifyLogExtensions.CompareMessages(\"a\", VerifyLogExpression, v.ToString())), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>())*");
        }

        [Fact]
        public void Verify_an_error_is_logged_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogWarning(new Exception("Something unexpected happened, but will survive."), "Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogWarning(new Exception("Something unexpected happened, but will survive."), "Test message"));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_an_error_by_a_condition_it_verifies()
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

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogWarning(It.Is<Exception>(e => e.Message == \"Some different message.\"), \"Test message\", new[] {  })*");
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
        public void Verify_null_error_any_message_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogWarning(null, "Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogWarning(null, It.IsAny<string>()));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_null_error_any_message_when_error_is_logged_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogWarning(new Exception("Some error message."), "Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogWarning(null, It.IsAny<string>()));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogWarning(null*")
                .Which.InnerException!.Message.Should().Match("*logger => logger.Log<It.IsAnyType>(LogLevel.Warning, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => True), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>())*");
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

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation(It.Is<string>(msg => msg.Contains(\"Expecting something else\")), new[] {  })*");
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

            act.Should().ThrowExactly<VerifyLogException>()
               .WithMessage("*.LogWarning(It.IsAny<Exception>(), null, new[] {  })*");
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

            act.Should().ThrowExactly<VerifyLogException>()
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
        public void Verify_it_is_regex_with_method_call_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogInformation(It.IsRegex(GetRegex())));

            act.Should().NotThrow();
        }
        string GetRegex() => "^(.*)$";

        [Fact]
        public void Verify_it_is_regex_when_message_does_not_match_the_regex_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(c => c.LogInformation(It.IsRegex("[0-9]")));

            act.Should().ThrowExactly<VerifyLogException>()
             .WithMessage("*.LogInformation*");
        }

        [Fact]
        public void Verify_event_id_is_logged_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation(new EventId(10, "Order"), "Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation(new EventId(10, "Order"), "Test message"));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_event_id_is_logged_when_mismatch_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation((EventId)10, "Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation((EventId)5, "Test message"));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation*5*");
        }

        [Fact]
        public void Verify_event_id_is_logged_when_It_is_used_it_verifies()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation((EventId)10, "Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation(It.Is<EventId>(eid => eid.Id == 10), "Test message"));

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_event_id_is_logged_when_It_is_used_and_mismatch_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation((EventId)10, "Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation(It.Is<EventId>(eid => eid.Id == 5), "Test message"));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*.LogInformation*5*");
        }

        [Fact]
        public void Verify_when_unsupported_extensions_are_used_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(c => c.SomeLoggerExtension());

            act.Should().ThrowExactly<NotSupportedException>()
                .WithMessage("Moq.ILogger supports*Microsoft.Extensions.Logging*SomeLoggerExtension*is not one of these*");
        }

        [Fact]
        public void Verify_when_unsupported_extensions_are_used_for_generic_logger_it_throws()
        {
            var loggerMock = new Mock<ILogger<object>>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(c => c.SomeLoggerExtension());

            act.Should().ThrowExactly<NotSupportedException>()
                .WithMessage("Moq.ILogger supports*Microsoft.Extensions.Logging*SomeLoggerExtension*is not one of these*");
        }

        [Fact]
        public void Verify_when_unexpected_error_happens_throws_an_unexpected_exception_with_an_explanatory_message()
        {
            var loggerMock = new Mock<ILogger>();
            var position = new { Latitude = 25, Longitude = 134 };
            var elapsedMs = 34;
            loggerMock.Object.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);

            Action act = () => loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", new { Latitude = It.Is<int>(lat => lat == 25), Longitude = 134 }, 34));

            act.Should().ThrowExactly<VerifyLogUnexpectedException>()
                .WithMessage("*https://github.com/adrianiftode/Moq.ILogger/issues/new*")
                .WithInnerException<Exception>();
        }

        [Fact]
        public void Verify_when_unsupported_expression_is_used_it_throws()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(c => new object());

            act.Should().ThrowExactly<NotSupportedException>()
                .WithMessage("Moq.ILogger supports*Microsoft.Extensions.Logging*A method name could not be resolved*");
        }

        [Fact]
        public void Verify_when_unsupported_unsupported_expression_is_used_for_generic_logger_it_throws()
        {
            var loggerMock = new Mock<ILogger<object>>();
            loggerMock.Object.LogInformation("Test message");

            Action act = () => loggerMock.VerifyLog(c => new object());

            act.Should().ThrowExactly<NotSupportedException>()
                .WithMessage("Moq.ILogger supports*Microsoft.Extensions.Logging*A method name could not be resolved*");
        }

        [Fact]
        public void Verify_includes_the_fail_message()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogDebug("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogDebug("Message"), "We expect to log `Message`.");

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*We expect to log `Message`.*");
        }

        [Fact]
        public void Verify_includes_the_fail_message_for_generic_logger()
        {
            var loggerMock = new Mock<ILogger<object>>();
            loggerMock.Object.LogDebug("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogDebug("Message"), "We expect to log `Message`.");

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*We expect to log `Message`.*");
        }

        [Fact]
        public void Verify_uses_Times()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogDebug("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogDebug("Test message"), Times.AtLeast(2));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*Expected invocation on the mock at least 2 times, but was 1 time*");
        }

        [Fact]
        public void Verify_includes_the_fail_message_with_Times()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogDebug("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogDebug("Test message"), Times.AtLeast(2), "We expect to log `Test message` at least twice.");

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*Expected invocation on the mock at least 2 times, but was 1 time*")
                .WithMessage("*We expect to log `Test message` at least twice.*");
        }

        [Fact]
        public void Verify_uses_Times_for_generic_logger()
        {
            var loggerMock = new Mock<ILogger<object>>();
            loggerMock.Object.LogDebug("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogDebug("Test message"), Times.AtLeast(2));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*Expected invocation on the mock at least 2 times, but was 1 time*");
        }

        [Fact]
        public void Verify_includes_the_fail_message_with_Times_for_generic_logger()
        {
            var loggerMock = new Mock<ILogger<object>>();
            loggerMock.Object.LogDebug("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogDebug("Test message"), Times.AtLeast(2), "We expect to log `Test message` at least twice.");

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*Expected invocation on the mock at least 2 times, but was 1 time*")
                .WithMessage("*We expect to log `Test message` at least twice.*");
        }

        [Fact]
        public void Verify_uses_Func_Times()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogDebug("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogDebug("Test message"), () => Times.AtLeast(2));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*Expected invocation on the mock at least 2 times, but was 1 time*");
        }

        [Fact]
        public void Verify_includes_the_fail_message_with_Func_Times()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Object.LogDebug("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogDebug("Test message"), () => Times.AtLeast(2), "We expect to log `Test message` at least twice.");

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*Expected invocation on the mock at least 2 times, but was 1 time*")
                .WithMessage("*We expect to log `Test message` at least twice.*");
        }

        [Fact]
        public void Verify_uses_Func_Times_for_generic_logger()
        {
            var loggerMock = new Mock<ILogger<object>>();
            loggerMock.Object.LogDebug("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogDebug("Test message"), () => Times.AtLeast(2));

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*Expected invocation on the mock at least 2 times, but was 1 time*");
        }

        [Fact]
        public void Verify_includes_the_fail_message_with_Func_Times_for_generic_logger()
        {
            var loggerMock = new Mock<ILogger<object>>();
            loggerMock.Object.LogDebug("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogDebug("Test message"), () => Times.AtLeast(2), "We expect to log `Test message` at least twice.");

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*Expected invocation on the mock at least 2 times, but was 1 time*")
                .WithMessage("*We expect to log `Test message` at least twice.*");
        }

        [Fact]
        public void Verify_uses_Times_Never()
        {
            var loggerMock = new Mock<ILogger<object>>();

            Action act = () => loggerMock.VerifyLog(logger => logger.LogDebug("Test message"), Times.Never());

            act.Should().NotThrow();
        }

        [Fact]
        public void Verify_includes_the_fail_message_with_Times_Never()
        {
            var loggerMock = new Mock<ILogger<object>>();
            loggerMock.Object.LogDebug("Test message");

            Action act = () => loggerMock.VerifyLog(logger => logger.LogDebug("Test message"), Times.Never);

            act.Should().ThrowExactly<VerifyLogException>()
                .WithMessage("*Expected invocation on the mock should never have been performed, but was 1 time*");
        }
    }

    internal static class OtherExtensions
    {
        public static void SomeLoggerExtension(this ILogger logger) { }
    }
}
