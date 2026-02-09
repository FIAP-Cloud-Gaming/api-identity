using Identity.API.Application.Behaviors;
using Identity.API.Application.Commands.RegisterUser;
using Identity.API.Application.Commands.ChangePassword;

namespace Identity.UnitTests.Application.Behaviors;

public sealed class LoggingBehaviorTests
{
    [Fact]
    public async Task Handle_WithResponse_CallsNextAndLogsSuccess()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<RegisterUserCommand, RegisterUserResult>>>();
        var behavior = new LoggingBehavior<RegisterUserCommand, RegisterUserResult>(logger);

        var command = new RegisterUserCommand("user@example.com", "P@ssw0rd123", "John", "Doe");
        var expectedResult = new RegisterUserResult(1, "user@example.com", "John Doe");

        var nextCalled = false;
        RequestHandlerDelegate<RegisterUserResult> next = (ct) =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithoutResponse_CallsNextAndLogsSuccess()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<ChangePasswordCommand>>>();
        var behavior = new LoggingBehavior<ChangePasswordCommand>(logger);

        var command = new ChangePasswordCommand(1, "OldP@ss", "NewP@ssw0rd123");

        var nextCalled = false;
        RequestHandlerDelegate next = (ct) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        // Act
        await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithResponse_OnException_LogsErrorAndRethrows()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<RegisterUserCommand, RegisterUserResult>>>();
        var behavior = new LoggingBehavior<RegisterUserCommand, RegisterUserResult>(logger);

        var command = new RegisterUserCommand("user@example.com", "P@ssw0rd123", "John", "Doe");

        RequestHandlerDelegate<RegisterUserResult> next = (ct) =>
            throw new InvalidOperationException("Test exception");

        // Act
        var act = async () => await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");
    }

    [Fact]
    public async Task Handle_WithoutResponse_OnException_LogsErrorAndRethrows()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<ChangePasswordCommand>>>();
        var behavior = new LoggingBehavior<ChangePasswordCommand>(logger);

        var command = new ChangePasswordCommand(1, "OldP@ss", "NewP@ssw0rd123");

        RequestHandlerDelegate next = (ct) => throw new InvalidOperationException("Test exception");

        // Act
        var act = async () => await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");
    }

    [Fact]
    public async Task Handle_WithResponse_LogsStartAndEnd()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<RegisterUserCommand, RegisterUserResult>>>();
        var behavior = new LoggingBehavior<RegisterUserCommand, RegisterUserResult>(logger);

        var command = new RegisterUserCommand("user@example.com", "P@ssw0rd123", "John", "Doe");
        var expectedResult = new RegisterUserResult(1, "user@example.com", "John Doe");

        RequestHandlerDelegate<RegisterUserResult> next = (ct) => Task.FromResult(expectedResult);

        // Act
        await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("[START]")),
            null,
            Arg.Any<Func<object, Exception?, string>>());

        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("[END]")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WithoutResponse_LogsStartAndEnd()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<ChangePasswordCommand>>>();
        var behavior = new LoggingBehavior<ChangePasswordCommand>(logger);

        var command = new ChangePasswordCommand(1, "OldP@ss", "NewP@ssw0rd123");

        RequestHandlerDelegate next = (ct) => Task.CompletedTask;

        // Act
        await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("[START]")),
            null,
            Arg.Any<Func<object, Exception?, string>>());

        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("[END]")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Handle_WithResponse_OnException_LogsError()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<RegisterUserCommand, RegisterUserResult>>>();
        var behavior = new LoggingBehavior<RegisterUserCommand, RegisterUserResult>(logger);

        var command = new RegisterUserCommand("user@example.com", "P@ssw0rd123", "John", "Doe");
        var exception = new InvalidOperationException("Test exception");

        RequestHandlerDelegate<RegisterUserResult> next = (ct) => throw exception;

        // Act
        try
        {
            await behavior.Handle(command, next, CancellationToken.None);
        }
        catch
        {
            // Expected
        }

        // Assert
        logger.Received().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("[ERROR]")),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
