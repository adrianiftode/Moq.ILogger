# Moq.ILogger
This is a [*Moq*](https://github.com/Moq/moq4/wiki/Quickstart) extension for *ILogger* in order to **Verify** the SUT interactions with the `ILogger` extensions using all the **Moq** goodies.

[![Build status](https://ci.appveyor.com/api/projects/status/iixn0pkeuuov1rwb/branch/master?svg=true)](https://ci.appveyor.com/project/adrianiftode/moq-ilogger/branch/master)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Moq.ILogger&metric=alert_status)](https://sonarcloud.io/dashboard?id=Moq.ILogger)
[![NuGet](https://img.shields.io/nuget/v/ILogger.Moq.svg)](https://www.nuget.org/packages/ILogger.Moq)

## Nuget
This package is stil in preview mode

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

Then the such SUT-`ILogger` interactions with can be verified:

```csharp
[Fact]
public void Verify_semantic_logging()
{
    var loggerMock = new Mock<ILogger<SomeClass>>();
    var sut = new SomeClass(loggerMock.Object);

    sut.SemanticLogging();

    loggerMock.VerifyLog(logger => logger.LogInformation("Processed {@Position} in {Elapsed:000} ms.", 
                                                          new { Latitude = 25, Longitude = 134 }, 34));
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
It is expected for the `VefifyLog` expression to use *ILogger* extensions methods, which is not normally possible with **Moq**.
Also notice the `VerifyLog` method is used and not the usual *Verify* method from **Moq**. 
If you use *Verify* instead of `VerifyLog` you'll then get a **Moq** exception with the following message `Invalid verify on an extension method`.

## Why
**Moq** cannot verify extension methods calls so you'll have to check the extension implementation and see what is actually called, then write the Moq Verify expression based on the implementation.

This package translates the given `VerifyLog` expression into one useful for **Moq** so it can pass it to the `ILogger.Log` method, which is part of the `ILogger` definition, and not an instance method.

When an extension method is passed to **Moq**, then an exception like the following one is raised.
```
  Message: 
    System.NotSupportedException : Invalid verify on an extension method: logger => logger.LogInformation("User is not authorized {user}", new[] {  })
  Stack Trace: 
    Mock.ThrowIfVerifyExpressionInvolvesUnsupportedMember(Expression verify, MethodInfo method)
    Mock.VerifyVoid(Mock mock, LambdaExpression expression, Times times, String failMessage)
    Mock`1.Verify(Expression`1 expression)
    AuthorizationTests.When_user_is_not_authorized_a_warning_containing_the_user_identity_is_logged()
```
