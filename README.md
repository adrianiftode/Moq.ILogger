# Moq.ILogger
Easy verify ILogger mocks

This is a [*Moq*](https://github.com/Moq/moq4/wiki/Quickstart) extension for *ILogger* in order help you to speed up the **Verify** (Assert) part and to extract enough information when **Fail** happens, so less time in debugging is spent.

[![Build status](https://ci.appveyor.com/api/projects/status/iixn0pkeuuov1rwb/branch/master?svg=true)](https://ci.appveyor.com/project/adrianiftode/moq-ilogger/branch/master)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Moq.ILogger&metric=alert_status)](https://sonarcloud.io/dashboard?id=Moq.ILogger)
[![NuGet](https://img.shields.io/nuget/v/ILogger.Mock.svg)](https://www.nuget.org/packages/ILogger.Mock)

## Nuget
This package is stil in preview mode

PM&gt; Install-Package ILogger.Moq

## Examples

Given the following SUT
```csharp
public class SomeClass
{
    private readonly ILogger<SomeClass> _logger;

    public SomeClass(ILogger<SomeClass> logger) => _logger = logger;

    public void LoggingInformation()
        => _logger.LogInformation("This operation is successfull.");

    public void LoggingWarning(string name)
        => _logger.LogWarning(new ArgumentException("The given name is not ok", nameof(name)), "This operation failed, but let's log an warning only");
}
```

You can verify the interactions with the `ILogger` extensions by actually using all Moq goodies

```csharp
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

##Why
```
  Message: 
    System.NotSupportedException : Invalid verify on an extension method: logger => logger.LogInformation("User is not authorized {user}", new[] {  })
  Stack Trace: 
    Mock.ThrowIfVerifyExpressionInvolvesUnsupportedMember(Expression verify, MethodInfo method) line 780
    Mock.VerifyVoid(Mock mock, LambdaExpression expression, Times times, String failMessage) line 276
    Mock`1.Verify(Expression`1 expression) line 408
    AuthorizationTests.When_user_is_not_authorized_a_warning_containing_the_user_identity_is_logged() line 28
```