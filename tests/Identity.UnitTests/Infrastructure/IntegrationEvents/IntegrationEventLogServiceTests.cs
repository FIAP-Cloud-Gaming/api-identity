using Identity.UnitTests.Helpers;

namespace Identity.UnitTests.Infrastructure.IntegrationEvents;

public sealed class IntegrationEventLogServiceTests : IDisposable
{
    private readonly IdentityContext _context;
    private readonly IIntegrationEventLogService _service;

    public IntegrationEventLogServiceTests()
    {
        _context = IdentityContextFactory.Create();
        _service = new IntegrationEventLogService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task SaveEventAsync_ShouldSaveEventToDatabase()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test" };
        var transactionId = Guid.NewGuid();

        // Act
        await _service.SaveEventAsync(testEvent, transactionId);

        // Assert
        var events = await _context.IntegrationEventLogs.ToListAsync();
        events.Should().HaveCount(1);
        events[0].TransactionId.Should().Be(transactionId);
        events[0].State.Should().Be(EventStateEnum.NotPublished);
    }

    [Fact]
    public async Task RetrieveEventLogsPendingToPublishAsync_ShouldReturnEmpty_WhenNoEventsExist()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        // Act
        var events = await _service.RetrieveEventLogsPendingToPublishAsync(transactionId);

        // Assert
        events.Should().BeEmpty();
    }

    [Fact]
    public async Task RetrieveEventLogsPendingToPublishAsync_ShouldReturnOnlyNotPublishedEvents()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var event1 = new TestEvent { Message = "Event1" };
        var event2 = new TestEvent { Message = "Event2" };
        var event3 = new TestEvent { Message = "Event3" };

        await _service.SaveEventAsync(event1, transactionId);
        await _service.SaveEventAsync(event2, transactionId);
        await _service.SaveEventAsync(event3, transactionId);

        var allEvents = await _context.IntegrationEventLogs.ToListAsync();
        allEvents[1].State = EventStateEnum.Published;
        await _context.SaveChangesAsync();

        // Act
        var pendingEvents = await _service.RetrieveEventLogsPendingToPublishAsync(transactionId);

        // Assert
        pendingEvents.Should().HaveCount(2);
        pendingEvents.All(e => e.State == EventStateEnum.NotPublished).Should().BeTrue();
    }

    [Fact]
    public async Task RetrieveEventLogsPendingToPublishAsync_ShouldFilterByTransactionId()
    {
        // Arrange
        var transactionId1 = Guid.NewGuid();
        var transactionId2 = Guid.NewGuid();

        await _service.SaveEventAsync(new TestEvent(), transactionId1);
        await _service.SaveEventAsync(new TestEvent(), transactionId1);
        await _service.SaveEventAsync(new TestEvent(), transactionId2);

        // Act
        var events = await _service.RetrieveEventLogsPendingToPublishAsync(transactionId1);

        // Assert
        events.Should().HaveCount(2);
        events.All(e => e.TransactionId == transactionId1).Should().BeTrue();
    }

    [Fact]
    public async Task RetrieveEventLogsPendingToPublishAsync_ShouldOrderByCreationTime()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        var event1 = new TestEvent { Message = "First" };
        var event2 = new TestEvent { Message = "Second" };
        var event3 = new TestEvent { Message = "Third" };

        await _service.SaveEventAsync(event1, transactionId);
        await Task.Delay(10);
        await _service.SaveEventAsync(event2, transactionId);
        await Task.Delay(10);
        await _service.SaveEventAsync(event3, transactionId);

        // Act
        var events = (await _service.RetrieveEventLogsPendingToPublishAsync(transactionId)).ToList();

        // Assert
        events.Should().HaveCount(3);
        events[0].Content.Should().Contain("First");
        events[1].Content.Should().Contain("Second");
        events[2].Content.Should().Contain("Third");
    }

    [Fact]
    public async Task MarkEventAsPublishedAsync_ShouldUpdateEventState()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        await _service.SaveEventAsync(new TestEvent(), transactionId);
        var events = await _context.IntegrationEventLogs.ToListAsync();
        var eventId = events[0].EventId;

        // Act
        await _service.MarkEventAsPublishedAsync(eventId);

        // Assert
        var updatedEvent = await _context.IntegrationEventLogs.FindAsync(eventId);
        updatedEvent.Should().NotBeNull();
        updatedEvent!.State.Should().Be(EventStateEnum.Published);
    }

    [Fact]
    public async Task MarkEventAsInProgressAsync_ShouldUpdateEventStateAndIncrementTimesSent()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        await _service.SaveEventAsync(new TestEvent(), transactionId);
        var events = await _context.IntegrationEventLogs.ToListAsync();
        var eventId = events[0].EventId;

        // Act
        await _service.MarkEventAsInProgressAsync(eventId);

        // Assert
        var updatedEvent = await _context.IntegrationEventLogs.FindAsync(eventId);
        updatedEvent.Should().NotBeNull();
        updatedEvent!.State.Should().Be(EventStateEnum.InProgress);
        updatedEvent.TimesSent.Should().Be(1);
    }

    [Fact]
    public async Task MarkEventAsInProgressAsync_ShouldIncrementTimesSentMultipleTimes()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        await _service.SaveEventAsync(new TestEvent(), transactionId);
        var events = await _context.IntegrationEventLogs.ToListAsync();
        var eventId = events[0].EventId;

        // Act
        await _service.MarkEventAsInProgressAsync(eventId);
        await _service.MarkEventAsInProgressAsync(eventId);
        await _service.MarkEventAsInProgressAsync(eventId);

        // Assert
        var updatedEvent = await _context.IntegrationEventLogs.FindAsync(eventId);
        updatedEvent.Should().NotBeNull();
        updatedEvent!.TimesSent.Should().Be(3);
    }

    [Fact]
    public async Task MarkEventAsFailedAsync_ShouldUpdateEventState()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        await _service.SaveEventAsync(new TestEvent(), transactionId);
        var events = await _context.IntegrationEventLogs.ToListAsync();
        var eventId = events[0].EventId;

        // Act
        await _service.MarkEventAsFailedAsync(eventId);

        // Assert
        var updatedEvent = await _context.IntegrationEventLogs.FindAsync(eventId);
        updatedEvent.Should().NotBeNull();
        updatedEvent!.State.Should().Be(EventStateEnum.PublishedFailed);
    }

    [Fact]
    public async Task MarkEventAsFailedAsync_ShouldNotIncrementTimesSent()
    {
        // Arrange
        var transactionId = Guid.NewGuid();
        await _service.SaveEventAsync(new TestEvent(), transactionId);
        var events = await _context.IntegrationEventLogs.ToListAsync();
        var eventId = events[0].EventId;

        // Act
        await _service.MarkEventAsFailedAsync(eventId);

        // Assert
        var updatedEvent = await _context.IntegrationEventLogs.FindAsync(eventId);
        updatedEvent.Should().NotBeNull();
        updatedEvent!.TimesSent.Should().Be(0);
    }

    [Fact]
    public async Task SaveEventAsync_ShouldHandleMultipleEvents()
    {
        // Arrange
        var transactionId = Guid.NewGuid();

        // Act
        await _service.SaveEventAsync(new TestEvent { Message = "Event1" }, transactionId);
        await _service.SaveEventAsync(new TestEvent { Message = "Event2" }, transactionId);
        await _service.SaveEventAsync(new TestEvent { Message = "Event3" }, transactionId);

        // Assert
        var events = await _context.IntegrationEventLogs.ToListAsync();
        events.Should().HaveCount(3);
    }

    private sealed class TestEvent
    {
        public string Message { get; set; } = string.Empty;
    }
}
