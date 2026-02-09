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

        var service = new IdentityIntegrationEventService(context, eventLogService, logger);

        var transactionId = Guid.NewGuid();
        var pendingEvents = new List<IntegrationEventLogEntry>
        {
            new IntegrationEventLogEntry(new TestIntegrationEvent(), transactionId),
            new IntegrationEventLogEntry(new TestIntegrationEvent(), transactionId)
        };

        eventLogService.RetrieveEventLogsPendingToPublishAsync(transactionId)
            .Returns(pendingEvents);

        // Act
        await service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        await eventLogService.Received(1).RetrieveEventLogsPendingToPublishAsync(transactionId);
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

        var service = new IdentityIntegrationEventService(context, eventLogService, logger);

        var transactionId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var pendingEvents = new List<IntegrationEventLogEntry>
        {
            new IntegrationEventLogEntry(new TestIntegrationEvent(), transactionId)
        };

        // Simula que o EventId é o mesmo que foi criado
        typeof(IntegrationEventLogEntry).GetProperty("EventId")!
            .SetValue(pendingEvents[0], eventId);

        eventLogService.RetrieveEventLogsPendingToPublishAsync(transactionId)
            .Returns(pendingEvents);

        eventLogService.MarkEventAsPublishedAsync(Arg.Any<Guid>())
            .Returns(Task.FromException(new InvalidOperationException("Publish failed")));

        // Act
        await service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        await eventLogService.Received(1).MarkEventAsFailedAsync(eventId);
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

        var service = new IdentityIntegrationEventService(context, eventLogService, logger);

        var transactionId = Guid.NewGuid();
        eventLogService.RetrieveEventLogsPendingToPublishAsync(transactionId)
            .Returns(new List<IntegrationEventLogEntry>());

        // Act
        await service.PublishEventsThroughEventBusAsync(transactionId);

        // Assert
        await eventLogService.Received(1).RetrieveEventLogsPendingToPublishAsync(transactionId);
        await eventLogService.DidNotReceive().MarkEventAsPublishedAsync(Arg.Any<Guid>());
        await eventLogService.DidNotReceive().MarkEventAsFailedAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task AddAndSaveEventAsync_WithActiveTransaction_SavesEvent()
    {
        // Arrange
        var context = IdentityContextFactory.Create();
        var eventLogService = Substitute.For<IIntegrationEventLogService>();
        var logger = Substitute.For<ILogger<IdentityIntegrationEventService>>();

        var service = new IdentityIntegrationEventService(context, eventLogService, logger);

        var @event = new TestIntegrationEvent();

        // Inicia uma transação
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

        var service = new IdentityIntegrationEventService(context, eventLogService, logger);

        var @event = new TestIntegrationEvent();

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

        var service = new IdentityIntegrationEventService(context, eventLogService, logger);

        var @event = new TestIntegrationEvent();

        // Inicia uma transação
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

        var service = new IdentityIntegrationEventService(context, eventLogService, logger);

        var transactionId = Guid.NewGuid();
        var pendingEvents = new List<IntegrationEventLogEntry>
        {
            new IntegrationEventLogEntry(new TestIntegrationEvent(), transactionId)
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

    private sealed record TestIntegrationEvent
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    }
}
