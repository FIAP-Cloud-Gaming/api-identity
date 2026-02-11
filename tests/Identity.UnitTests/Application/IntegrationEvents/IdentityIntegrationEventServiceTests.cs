using Identity.API.Application.IntegrationEvents;
using Identity.Infrastructure.IntegrationEvents;
using Identity.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore.Storage;

namespace Identity.UnitTests.Application.IntegrationEvents;

public sealed class IdentityIntegrationEventServiceTests
{
    [Fact]
    public async Task PublishEventsThroughEventBusAsync_RetrievesPendingEventsAndMarksPublished()
    {
        // Arrange
        var context = Substitute.For<IdentityContext>(
            new DbContextOptionsBuilder<IdentityContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            Substitute.For<IMediator>());

        var eventLogService = Substitute.For<IIntegrationEventLogService>();
        var logger = Substitute.For<ILogger<IdentityIntegrationEventService>>();
        var eventBus = Substitute.For<IEventBus>();

        var service = new IdentityIntegrationEventService(context, eventBus, eventLogService, logger);

        var transactionId = Guid.NewGuid();
        var pendingEvents = new List<IntegrationEventLogEntry>
        {
            new IntegrationEventLogEntry(new UserRegisteredIntegrationEvent(1, "user1@test.com", "User One"), transactionId),
            new IntegrationEventLogEntry(new UserRegisteredIntegrationEvent(2, "user2@test.com", "User Two"), transactionId)
        };

        eventLogService.RetrieveEventLogsPendingToPublishAsync(transactionId)
            .Returns(pendingEvents);

        // Act
        await service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        await eventLogService.Received(1).RetrieveEventLogsPendingToPublishAsync(transactionId);
        await eventLogService.Received(pendingEvents.Count)
            .MarkEventAsInProgressAsync(Arg.Any<Guid>());
        await eventBus.Received(pendingEvents.Count)
            .PublishAsync(Arg.Any<IntegrationEvent>());
        await eventLogService.Received(pendingEvents.Count)
            .MarkEventAsPublishedAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task PublishEventsThroughEventBusAsync_WhenErrorOccurs_MarksEventAsFailed()
    {
        // Arrange
        var context = Substitute.For<IdentityContext>(
            new DbContextOptionsBuilder<IdentityContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            Substitute.For<IMediator>());

        var eventLogService = Substitute.For<IIntegrationEventLogService>();
        var logger = Substitute.For<ILogger<IdentityIntegrationEventService>>();
        var eventBus = Substitute.For<IEventBus>();

        var service = new IdentityIntegrationEventService(context, eventBus, eventLogService, logger);

        var transactionId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var pendingEvents = new List<IntegrationEventLogEntry>
        {
            new IntegrationEventLogEntry(new UserActivatedIntegrationEvent(1, "user@test.com"), transactionId)
        };

        typeof(IntegrationEventLogEntry).GetProperty("EventId")!
            .SetValue(pendingEvents[0], eventId);

        eventLogService.RetrieveEventLogsPendingToPublishAsync(transactionId)
            .Returns(pendingEvents);

        eventBus.PublishAsync(Arg.Any<IntegrationEvent>())
            .Returns(Task.FromException(new InvalidOperationException("Publish failed")));

        // Act
        await service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        await eventLogService.Received(1).MarkEventAsInProgressAsync(eventId);
        await eventLogService.Received(1).MarkEventAsFailedAsync(eventId);
        await eventLogService.DidNotReceive().MarkEventAsPublishedAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task PublishEventsThroughEventBusAsync_NoPendingEvents_DoesNothing()
    {
        // Arrange
        var context = Substitute.For<IdentityContext>(
            new DbContextOptionsBuilder<IdentityContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            Substitute.For<IMediator>());

        var eventLogService = Substitute.For<IIntegrationEventLogService>();
        var logger = Substitute.For<ILogger<IdentityIntegrationEventService>>();
        var eventBus = Substitute.For<IEventBus>();

        var service = new IdentityIntegrationEventService(context, eventBus, eventLogService, logger);

        var transactionId = Guid.NewGuid();
        eventLogService.RetrieveEventLogsPendingToPublishAsync(transactionId)
            .Returns(new List<IntegrationEventLogEntry>());

        // Act
        await service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        await eventLogService.Received(1).RetrieveEventLogsPendingToPublishAsync(transactionId);
        await eventLogService.DidNotReceive().MarkEventAsPublishedAsync(Arg.Any<Guid>());
        await eventLogService.DidNotReceive().MarkEventAsFailedAsync(Arg.Any<Guid>());
        await eventBus.DidNotReceive().PublishAsync(Arg.Any<IntegrationEvent>());
    }

    [Fact]
    public async Task AddAndSaveEventAsync_WithActiveTransaction_SavesEvent()
    {
        // Arrange
        var context = IdentityContextFactory.Create();
        var eventLogService = Substitute.For<IIntegrationEventLogService>();
        var logger = Substitute.For<ILogger<IdentityIntegrationEventService>>();
        var eventBus = Substitute.For<IEventBus>();

        var service = new IdentityIntegrationEventService(context, eventBus, eventLogService, logger);

        var @event = new UserRegisteredIntegrationEvent(1, "test@test.com", "Test User");

        // Inicia uma transacao
        var transaction = await context.BeginTransactionAsync();

        // Act
        await service.AddAndSaveEventAsync(@event);

        // Assert
        await eventLogService.Received(1).SaveEventAsync(@event, transaction!.TransactionId);
    }

    [Fact]
    public async Task AddAndSaveEventAsync_WithoutActiveTransaction_ThrowsInvalidOperationException()
    {
        // Arrange
        var context = IdentityContextFactory.Create();
        var eventLogService = Substitute.For<IIntegrationEventLogService>();
        var logger = Substitute.For<ILogger<IdentityIntegrationEventService>>();
        var eventBus = Substitute.For<IEventBus>();

        var service = new IdentityIntegrationEventService(context, eventBus, eventLogService, logger);

        var @event = new UserRegisteredIntegrationEvent(1, "test@test.com", "Test User");

        // Act
        var act = async () => await service.AddAndSaveEventAsync(@event);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*transaction must be active*");
    }

    [Fact]
    public async Task AddAndSaveEventAsync_LogsEventInformation()
    {
        // Arrange
        var context = IdentityContextFactory.Create();
        var eventLogService = Substitute.For<IIntegrationEventLogService>();
        var logger = Substitute.For<ILogger<IdentityIntegrationEventService>>();
        var eventBus = Substitute.For<IEventBus>();

        var service = new IdentityIntegrationEventService(context, eventBus, eventLogService, logger);

        var @event = new UserRegisteredIntegrationEvent(1, "test@test.com", "Test User");

        // Inicia uma transacao
        await context.BeginTransactionAsync();

        // Act
        await service.AddAndSaveEventAsync(@event);

        // Assert
        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Enqueuing integration event")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task PublishEventsThroughEventBusAsync_LogsPublishingInformation()
    {
        // Arrange
        var context = Substitute.For<IdentityContext>(
            new DbContextOptionsBuilder<IdentityContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options,
            Substitute.For<IMediator>());

        var eventLogService = Substitute.For<IIntegrationEventLogService>();
        var logger = Substitute.For<ILogger<IdentityIntegrationEventService>>();
        var eventBus = Substitute.For<IEventBus>();

        var service = new IdentityIntegrationEventService(context, eventBus, eventLogService, logger);

        var transactionId = Guid.NewGuid();
        var pendingEvents = new List<IntegrationEventLogEntry>
        {
            new IntegrationEventLogEntry(new UserDeactivatedIntegrationEvent(1, "test@test.com"), transactionId)
        };

        eventLogService.RetrieveEventLogsPendingToPublishAsync(transactionId)
            .Returns(pendingEvents);

        // Act
        await service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Publishing integration event")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }
}
