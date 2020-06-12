using System;
using FluentAssertions;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Moq.Tests
{
    public class VerifyLogExceptionTests
    {
        [Fact]
        public void Ctor_DoesNotThrow()
        {
            Action act = () => new VerifyLogException();

            act.Should().NotThrow();
        }

        [Fact]
        public void Ctor_WhenMessage_DoesNotThrow()
        {
            Action act = () => new VerifyLogException("Some message");

            act.Should().NotThrow();
        }

        [Fact]
        public void Ctor_WhenInnerException_DoesNotThrow()
        {
            Action act = () => new VerifyLogException("Some message", new Exception());

            act.Should().NotThrow();
        }

        [Fact]
        public void Exception_ShouldBeBinarySerializable()
        {
            var sut = new VerifyLogException("Some message", new Exception());

            sut.Should().BeBinarySerializable();
        }
    }
}