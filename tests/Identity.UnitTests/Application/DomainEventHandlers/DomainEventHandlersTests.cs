using Identity.API.Application.DomainEventHandlers;

namespace Identity.UnitTests.Application.DomainEventHandlers;

public sealed class DomainEventHandlersTests
{
    [Fact]
    public async Task UserRegisteredEventHandler_Handle_CompletesSuccessfully()
    {
        // Arrange
        var integrationEventService = Substitute.For<IIdentityIntegrationEventService>();
        var logger = NullLogger<UserRegisteredEventHandler>.Instance;
        var handler = new UserRegisteredEventHandler(integrationEventService, logger);
        var user = User.Create(
            new Email("user@example.com"),
            Password.FromHash("hashed_password"),
            new PersonName("John", "Doe"));
        var domainEvent = new UserRegisteredDomainEvent(user);

        // Act
        var act = async () => await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PasswordChangedEventHandler_Handle_CompletesSuccessfully()
    {
        // Arrange
        var logger = NullLogger<PasswordChangedEventHandler>.Instance;
        var handler = new PasswordChangedEventHandler(logger);
        var domainEvent = new PasswordChangedDomainEvent(1, "user@example.com");

        // Act
        var act = async () => await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UserLoggedInEventHandler_Handle_CompletesSuccessfully()
    {
        // Arrange
        var logger = NullLogger<UserLoggedInEventHandler>.Instance;
        var handler = new UserLoggedInEventHandler(logger);
        var domainEvent = new UserLoggedInDomainEvent(1, "user@example.com", DateTime.UtcNow);

        // Act
        var act = async () => await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UserActivatedEventHandler_Handle_CompletesSuccessfully()
    {
        // Arrange
        var integrationEventService = Substitute.For<IIdentityIntegrationEventService>();
        var logger = NullLogger<UserActivatedEventHandler>.Instance;
        var handler = new UserActivatedEventHandler(integrationEventService, logger);
        var domainEvent = new UserActivatedDomainEvent(1, "user@example.com");

        // Act
        var act = async () => await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UserDeactivatedEventHandler_Handle_CompletesSuccessfully()
    {
        // Arrange
        var integrationEventService = Substitute.For<IIdentityIntegrationEventService>();
        var logger = NullLogger<UserDeactivatedEventHandler>.Instance;
        var handler = new UserDeactivatedEventHandler(integrationEventService, logger);
        var domainEvent = new UserDeactivatedDomainEvent(1, "user@example.com");

        // Act
        var act = async () => await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RoleAssignedEventHandler_Handle_CompletesSuccessfully()
    {
        // Arrange
        var logger = NullLogger<RoleAssignedEventHandler>.Instance;
        var handler = new RoleAssignedEventHandler(logger);
        var domainEvent = new RoleAssignedDomainEvent(1, "ADMIN");

        // Act
        var act = async () => await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RoleRemovedEventHandler_Handle_CompletesSuccessfully()
    {
        // Arrange
        var logger = NullLogger<RoleRemovedEventHandler>.Instance;
        var handler = new RoleRemovedEventHandler(logger);
        var domainEvent = new RoleRemovedDomainEvent(1, "ADMIN");

        // Act
        var act = async () => await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RefreshTokenGeneratedEventHandler_Handle_CompletesSuccessfully()
    {
        // Arrange
        var logger = NullLogger<RefreshTokenGeneratedEventHandler>.Instance;
        var handler = new RefreshTokenGeneratedEventHandler(logger);
        var domainEvent = new RefreshTokenGeneratedDomainEvent(1, DateTime.UtcNow.AddDays(7));

        // Act
        var act = async () => await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RefreshTokenRevokedEventHandler_Handle_CompletesSuccessfully()
    {
        // Arrange
        var logger = NullLogger<RefreshTokenRevokedEventHandler>.Instance;
        var handler = new RefreshTokenRevokedEventHandler(logger);
        var domainEvent = new RefreshTokenRevokedDomainEvent(1, "Manual revocation");

        // Act
        var act = async () => await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AllHandlers_HandleWithCancellation_CompletesSuccessfully()
    {
        // Arrange
        var cts = new CancellationTokenSource();

        // Act & Assert - Test all handlers with cancellation token
        var testUser = User.Create(
            new Email("test@example.com"),
            Password.FromHash("hashed_password"),
            new PersonName("Test", "User"));

        var integrationEventService = Substitute.For<IIdentityIntegrationEventService>();

        await new UserRegisteredEventHandler(integrationEventService, NullLogger<UserRegisteredEventHandler>.Instance)
            .Handle(new UserRegisteredDomainEvent(testUser), cts.Token);

        await new PasswordChangedEventHandler(NullLogger<PasswordChangedEventHandler>.Instance)
            .Handle(new PasswordChangedDomainEvent(1, "test@example.com"), cts.Token);

        await new UserLoggedInEventHandler(NullLogger<UserLoggedInEventHandler>.Instance)
            .Handle(new UserLoggedInDomainEvent(1, "test@example.com", DateTime.UtcNow), cts.Token);

        await new UserActivatedEventHandler(integrationEventService, NullLogger<UserActivatedEventHandler>.Instance)
            .Handle(new UserActivatedDomainEvent(1, "test@example.com"), cts.Token);

        await new UserDeactivatedEventHandler(integrationEventService, NullLogger<UserDeactivatedEventHandler>.Instance)
            .Handle(new UserDeactivatedDomainEvent(1, "test@example.com"), cts.Token);

        await new RoleAssignedEventHandler(NullLogger<RoleAssignedEventHandler>.Instance)
            .Handle(new RoleAssignedDomainEvent(1, "ADMIN"), cts.Token);

        await new RoleRemovedEventHandler(NullLogger<RoleRemovedEventHandler>.Instance)
            .Handle(new RoleRemovedDomainEvent(1, "ADMIN"), cts.Token);

        await new RefreshTokenGeneratedEventHandler(NullLogger<RefreshTokenGeneratedEventHandler>.Instance)
            .Handle(new RefreshTokenGeneratedDomainEvent(1, DateTime.UtcNow.AddDays(7)), cts.Token);

        await new RefreshTokenRevokedEventHandler(NullLogger<RefreshTokenRevokedEventHandler>.Instance)
            .Handle(new RefreshTokenRevokedDomainEvent(1, "Test"), cts.Token);

        // If we reach here, all handlers completed successfully
        true.Should().BeTrue();
    }
}
