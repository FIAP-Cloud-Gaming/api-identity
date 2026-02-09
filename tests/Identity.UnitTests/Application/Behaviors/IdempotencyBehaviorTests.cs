using Identity.API.Application.Behaviors;
using Identity.API.Application.Commands.Identified;
using Identity.API.Application.Commands.RegisterUser;
using Identity.API.Application.Commands.ChangePassword;

namespace Identity.UnitTests.Application.Behaviors;

public sealed class IdempotencyBehaviorTests
{
    [Fact]
    public async Task Handle_WithResponse_NonIdentifiedCommand_PassesThrough()
    {
        // Arrange
        var requestManager = Substitute.For<IRequestManager>();
        var logger = Substitute.For<ILogger<IdempotencyBehavior<RegisterUserCommand, RegisterUserResult>>>();
        var behavior = new IdempotencyBehavior<RegisterUserCommand, RegisterUserResult>(requestManager, logger);

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
        await requestManager.DidNotReceive().ExistAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_WithoutResponse_NonIdentifiedCommand_PassesThrough()
    {
        // Arrange
        var requestManager = Substitute.For<IRequestManager>();
        var logger = Substitute.For<ILogger<IdempotencyBehavior<ChangePasswordCommand>>>();
        var behavior = new IdempotencyBehavior<ChangePasswordCommand>(requestManager, logger);

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
        await requestManager.DidNotReceive().ExistAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_WithResponse_NewIdentifiedCommand_CreatesRequestAndProcesses()
    {
        // Arrange
        var requestManager = Substitute.For<IRequestManager>();
        var logger = Substitute.For<ILogger<IdempotencyBehavior<IdentifiedCommand<RegisterUserCommand, RegisterUserResult>, RegisterUserResult>>>();
        var behavior = new IdempotencyBehavior<IdentifiedCommand<RegisterUserCommand, RegisterUserResult>, RegisterUserResult>(requestManager, logger);

        var innerCommand = new RegisterUserCommand("user@example.com", "P@ssw0rd123", "John", "Doe");
        var requestId = Guid.NewGuid();
        var command = new IdentifiedCommand<RegisterUserCommand, RegisterUserResult>(innerCommand, requestId);
        var expectedResult = new RegisterUserResult(1, "user@example.com", "John Doe");

        requestManager.ExistAsync(requestId).Returns(false);

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
        await requestManager.Received(1).ExistAsync(requestId);
        await requestManager.Received(1).CreateRequestForCommandAsync<IdentifiedCommand<RegisterUserCommand, RegisterUserResult>>(requestId);
    }

    [Fact]
    public async Task Handle_WithoutResponse_NewIdentifiedCommand_CreatesRequestAndProcesses()
    {
        // Arrange
        var requestManager = Substitute.For<IRequestManager>();
        var logger = Substitute.For<ILogger<IdempotencyBehavior<IdentifiedCommand<ChangePasswordCommand>>>>();
        var behavior = new IdempotencyBehavior<IdentifiedCommand<ChangePasswordCommand>>(requestManager, logger);

        var innerCommand = new ChangePasswordCommand(1, "OldP@ss", "NewP@ssw0rd123");
        var requestId = Guid.NewGuid();
        var command = new IdentifiedCommand<ChangePasswordCommand>(innerCommand, requestId);

        requestManager.ExistAsync(requestId).Returns(false);

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
        await requestManager.Received(1).ExistAsync(requestId);
        await requestManager.Received(1).CreateRequestForCommandAsync<IdentifiedCommand<ChangePasswordCommand>>(requestId);
    }

    [Fact]
    public async Task Handle_WithResponse_DuplicateIdentifiedCommand_ReturnsDefault()
    {
        // Arrange
        var requestManager = Substitute.For<IRequestManager>();
        var logger = Substitute.For<ILogger<IdempotencyBehavior<IdentifiedCommand<RegisterUserCommand, RegisterUserResult>, RegisterUserResult>>>();
        var behavior = new IdempotencyBehavior<IdentifiedCommand<RegisterUserCommand, RegisterUserResult>, RegisterUserResult>(requestManager, logger);

        var innerCommand = new RegisterUserCommand("user@example.com", "P@ssw0rd123", "John", "Doe");
        var requestId = Guid.NewGuid();
        var command = new IdentifiedCommand<RegisterUserCommand, RegisterUserResult>(innerCommand, requestId);

        requestManager.ExistAsync(requestId).Returns(true);

        var nextCalled = false;
        RequestHandlerDelegate<RegisterUserResult> next = (ct) =>
        {
            nextCalled = true;
            return Task.FromResult(new RegisterUserResult(1, "user@example.com", "John Doe"));
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        nextCalled.Should().BeFalse();
        await requestManager.Received(1).ExistAsync(requestId);
        await requestManager.DidNotReceive().CreateRequestForCommandAsync<IdentifiedCommand<RegisterUserCommand, RegisterUserResult>>(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_WithoutResponse_DuplicateIdentifiedCommand_DoesNotProcess()
    {
        // Arrange
        var requestManager = Substitute.For<IRequestManager>();
        var logger = Substitute.For<ILogger<IdempotencyBehavior<IdentifiedCommand<ChangePasswordCommand>>>>();
        var behavior = new IdempotencyBehavior<IdentifiedCommand<ChangePasswordCommand>>(requestManager, logger);

        var innerCommand = new ChangePasswordCommand(1, "OldP@ss", "NewP@ssw0rd123");
        var requestId = Guid.NewGuid();
        var command = new IdentifiedCommand<ChangePasswordCommand>(innerCommand, requestId);

        requestManager.ExistAsync(requestId).Returns(true);

        var nextCalled = false;
        RequestHandlerDelegate next = (ct) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        // Act
        await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
        await requestManager.Received(1).ExistAsync(requestId);
        await requestManager.DidNotReceive().CreateRequestForCommandAsync<IdentifiedCommand<ChangePasswordCommand>>(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_WithResponse_DuplicateBooleanCommand_ReturnsTrue()
    {
        // Arrange
        var requestManager = Substitute.For<IRequestManager>();
        var logger = Substitute.For<ILogger<IdempotencyBehavior<IdentifiedCommand<TestBoolCommand, bool>, bool>>>();
        var behavior = new IdempotencyBehavior<IdentifiedCommand<TestBoolCommand, bool>, bool>(requestManager, logger);

        var innerCommand = new TestBoolCommand();
        var requestId = Guid.NewGuid();
        var command = new IdentifiedCommand<TestBoolCommand, bool>(innerCommand, requestId);

        requestManager.ExistAsync(requestId).Returns(true);

        RequestHandlerDelegate<bool> next = (ct) => Task.FromResult(true);

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    public sealed record TestBoolCommand : ICommand<bool>;
}
