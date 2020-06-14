# Moq.ILogger

Enables the [*Moq's*](https://github.com/Moq/moq4/wiki/Quickstart) **Verify** API over the **ILogger** extensions (LogInformation, LogError, etc).

[![Build status](https://ci.appveyor.com/api/projects/status/iixn0pkeuuov1rwb/branch/master?svg=true)](https://ci.appveyor.com/project/adrianiftode/moq-ilogger/branch/master)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Moq.ILogger&metric=alert_status)](https://sonarcloud.io/dashboard?id=Moq.ILogger)
[![NuGet](https://img.shields.io/nuget/v/ILogger.Moq.svg)](https://www.nuget.org/packages/ILogger.Moq)

## Nuget

**PM&gt; Install-Package ILogger.Moq**

## Examples

Given the following SUT:

```csharp
public class SomeClass
{
    private readonly ILogger<SomeClass> _logger;

    public SomeClass(ILogger<SomeClass> logger) => _logger = logger;

    public void SemanticLogging()
    {
        var position = new { Latitude = 25, Longitude = 134 };
        var elapsedMs = 34;

         _logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", position, elapsedMs);
    }

    public void LoggingInformation()
        => _logger.LogInformation("This operation is successful.");

    public void LoggingWarning(string name)
        => _logger.LogWarning(new ArgumentException("The given name is not ok", nameof(name)), "This operation failed, but let's log an warning only");
}
```

Then interactions like the ones bellow can be asserted:

```csharp
[Fact]
public void Verify_semantic_logging()
{
    var loggerMock = new Mock<ILogger<SomeClass>>();
    var sut = new SomeClass(loggerMock.Object);

    sut.SemanticLogging();

    loggerMock.VerifyLog(logger => logger.LogInformation("Processed { Latitude = 25, Longitude = 134 } in 034 ms."));

    loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", new { Latitude = 25, Longitude = 134 }, 34));
    loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", It.IsAny<It.IsAnyType>(), It.IsAny<int>()));

    // wildcard usages
    loggerMock.VerifyLog(logger => logger.LogInformation("Processed { Latitude = *, Longitude = * } in * ms."));
    loggerMock.VerifyLog(logger => logger.LogInformation("Processed * in * ms."));
    loggerMock.VerifyLog(logger => logger.LogInformation("Processed*{@Position}*{Elapsed:000}*ms."));

    loggerMock.VerifyLog(logger => logger.LogInformation("Processed * in * ms.", It.IsAny<It.IsAnyType>(), It.IsAny<int>()));
    loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position}*{Elapsed:000} ms.", new { Latitude = 25, Longitude = 134 }, 34));
}
```

```csharp
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
```

```csharp
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

```
It is expected for the `VefifyLog` expression to contain *ILogger* extensions methods, which is not possible with **Moq**.
If you use *Verify*, which is part of the **Moq** library, instead of the `VerifyLog` method, then you'll then get a **Moq** exception with the following message `Invalid verify on an extension method`.

## Why
**Moq** cannot verify extension methods calls, as in essence these are static classes and cannot be mocked, so you'll have to check the extension implementation and see what is actually called. Once you figure out, then you have to write the **Moq** *Verify* expression based on the extension's implementation as you have to see which interface method is hit.

When an extension method is passed to **Moq**, then an exception like the following one is raised:
```
  Message: 
    System.NotSupportedException : Invalid verify on an extension method: logger => logger.LogInformation("User is not authorized {user}", new[] {  })
  Stack Trace: 
    Mock.ThrowIfVerifyExpressionInvolvesUnsupportedMember(Expression verify, MethodInfo method)
    Mock.VerifyVoid(Mock mock, LambdaExpression expression, Times times, String failMessage)
    Mock`1.Verify(Expression`1 expression)
    AuthorizationTests.When_user_is_not_authorized_a_warning_containing_the_user_identity_is_logged()
```

This package translates the given `VerifyLog` expression into one expected by the `ILogger.Log` method, which is the actual method that is called by the [LoggerExtensions](https://github.com/dotnet/runtime/blob/e3ffd343ad5bd3a999cb9515f59e6e7a777b2c34/src/libraries/Microsoft.Extensions.Logging.Abstractions/src/LoggerExtensions.cs) class.

There are three main benefits using this package:
- it is easier to verify the interaction with ILogger;
- the verification in tests are alike the SUT's implementation;
- facilitates the investigation of the failed tests.


## Failed test messages

When a test fails because the SUT does not behave as specified in the *VerifyLog* setup, then the message will contain a representation of the `VerifyLog` expression and also the original **Moq** exception message.

This is a failed test example that displays the VerifyLog expression with the expected setup, followed by the original Moq exception.

```
 Moq.Tests.Samples.SomeClassTest.Verify_semantic_logging
   Source: SomeClassTest.cs line 64
   Duration: 645 ms

  Message: 
    Moq.VerifyLogException : 
    Expected invocation on the mock at least once, but was never performed: logger => logger.LogInformation("Processed {@Position} * {Elapsed:000} ms.", new[] { new {int Latitude, int Longitude}(25, 134), (object)34 })
    
    
    ---- Moq.MockException : 
    Expected invocation on the mock at least once, but was never performed: logger => logger.Log<It.IsAnyType>(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => VerifyLogExtensions.CompareMessages("Processed {@Position} * {Elapsed:000} ms.", VerifyLogExpression, v)), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>())
    
    Performed invocations:
    
       Mock<ILogger<SomeClass>:1> (logger):
    
          ILogger.Log<object>(LogLevel.Information, 0, Processed { Latitude = 0, Longitude = 0 } in 000 ms., null, Func<object, Exception, string>)
    
  Stack Trace: 
    VerifyLogExtensions.Verify[T](Mock`1 loggerMock, Expression`1 expression, Nullable`1 times, Func`1 timesFunc, String failMessage) line 234
    VerifyLogExtensions.VerifyLog[T](Mock`1 loggerMock, Expression`1 expression) line 126
    SomeClassTest.Verify_semantic_logging() line 73
    ----- Inner Stack Trace -----
    Mock.Verify(Mock mock, LambdaExpression expression, Times times, String failMessage)
    Mock`1.Verify(Expression`1 expression, String failMessage)
    VerifyLogExtensions.Verify[T](Mock`1 loggerMock, Expression`1 expression, Nullable`1 times, Func`1 timesFunc, String failMessage) line 230

```