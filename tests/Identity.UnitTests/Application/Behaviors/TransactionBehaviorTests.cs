using Identity.API.Application.Behaviors;
using Identity.API.Application.Commands.RegisterUser;
using Identity.API.Application.Commands.ChangePassword;
using Identity.API.Application.IntegrationEvents;
using Identity.UnitTests.Helpers;

namespace Identity.UnitTests.Application.Behaviors;

public sealed class TransactionBehaviorTests
{
    [Fact]
    public async Task Handle_WithResponse_WhenHasActiveTransaction_CallsNextDirectly()
    {
        // Arrange
        var context = IdentityContextFactory.Create();
        var integrationEventService = Substitute.For<IIdentityIntegrationEventService>();
        var logger = Substitute.For<ILogger<TransactionBehavior<RegisterUserCommand, RegisterUserResult>>>();

        var behavior = new TransactionBehavior<RegisterUserCommand, RegisterUserResult>(
            context,
            integrationEventService,
            logger);

        var command = new RegisterUserCommand("user@example.com", "P@ssw0rd123", "John", "Doe");
        var expectedResult = new RegisterUserResult(1, "user@example.com", "John Doe");

        var nextCalled = false;
        RequestHandlerDelegate<RegisterUserResult> next = (ct) =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResult);
        };

        // Simula transação ativa
        await context.BeginTransactionAsync();

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        nextCalled.Should().BeTrue();
        await integrationEventService.DidNotReceive()
            .PublishEventsThroughEventBusAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_WithoutResponse_WhenHasActiveTransaction_CallsNextDirectly()
    {
        // Arrange
        var context = IdentityContextFactory.Create();
        var integrationEventService = Substitute.For<IIdentityIntegrationEventService>();
        var logger = Substitute.For<ILogger<TransactionBehavior<ChangePasswordCommand>>>();

        var behavior = new TransactionBehavior<ChangePasswordCommand>(
            context,
            integrationEventService,
            logger);

        var command = new ChangePasswordCommand(1, "OldP@ss", "NewP@ssw0rd123");

        var nextCalled = false;
        RequestHandlerDelegate next = (ct) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        // Simula transação ativa
        await context.BeginTransactionAsync();

        // Act
        await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        await integrationEventService.DidNotReceive()
            .PublishEventsThroughEventBusAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_WithResponse_WhenNoActiveTransaction_CreatesTransactionAndPublishesEvents()
    {
        // Arrange
        var context = IdentityContextFactory.Create();
        var integrationEventService = Substitute.For<IIdentityIntegrationEventService>();
        var logger = Substitute.For<ILogger<TransactionBehavior<RegisterUserCommand, RegisterUserResult>>>();

        var behavior = new TransactionBehavior<RegisterUserCommand, RegisterUserResult>(
            context,
            integrationEventService,
            logger);

        var command = new RegisterUserCommand("user@example.com", "P@ssw0rd123", "John", "Doe");
        var expectedResult = new RegisterUserResult(1, "user@example.com", "John Doe");

        var nextCalled = false;
        RequestHandlerDelegate<RegisterUserResult> next = async (ct) =>
        {
            nextCalled = true;
            var user = UserTestFactory.CreateValidUser("user@example.com", "John", "Doe", "hash");
            context.Users.Add(user);
            await context.SaveChangesAsync(ct);
            return expectedResult;
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        nextCalled.Should().BeTrue();
        await integrationEventService.Received(1)
            .PublishEventsThroughEventBusAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_WithoutResponse_WhenNoActiveTransaction_CreatesTransactionAndPublishesEvents()
    {
        // Arrange
        var context = IdentityContextFactory.Create();
        var integrationEventService = Substitute.For<IIdentityIntegrationEventService>();
        var logger = Substitute.For<ILogger<TransactionBehavior<ChangePasswordCommand>>>();

        var behavior = new TransactionBehavior<ChangePasswordCommand>(
            context,
            integrationEventService,
            logger);

        var command = new ChangePasswordCommand(1, "OldP@ss", "NewP@ssw0rd123");

        var nextCalled = false;
        RequestHandlerDelegate next = async (ct) =>
        {
            nextCalled = true;
            var user = UserTestFactory.CreateValidUser("user@example.com", "John", "Doe", "hash");
            context.Users.Add(user);
            await context.SaveChangesAsync(ct);
        };

        // Act
        await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        await integrationEventService.Received(1)
            .PublishEventsThroughEventBusAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_WithResponse_WhenExceptionOccurs_LogsAndRethrows()
    {
        // Arrange
        var context = IdentityContextFactory.Create();
        var integrationEventService = Substitute.For<IIdentityIntegrationEventService>();
        var logger = Substitute.For<ILogger<TransactionBehavior<RegisterUserCommand, RegisterUserResult>>>();

        var behavior = new TransactionBehavior<RegisterUserCommand, RegisterUserResult>(
            context,
            integrationEventService,
            logger);

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
    public async Task Handle_WithoutResponse_WhenExceptionOccurs_LogsAndRethrows()
    {
        // Arrange
        var context = IdentityContextFactory.Create();
        var integrationEventService = Substitute.For<IIdentityIntegrationEventService>();
        var logger = Substitute.For<ILogger<TransactionBehavior<ChangePasswordCommand>>>();

        var behavior = new TransactionBehavior<ChangePasswordCommand>(
            context,
            integrationEventService,
            logger);

        var command = new ChangePasswordCommand(1, "OldP@ss", "NewP@ssw0rd123");

        RequestHandlerDelegate next = (ct) => throw new InvalidOperationException("Test exception");

        // Act
        var act = async () => await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");
    }
}
