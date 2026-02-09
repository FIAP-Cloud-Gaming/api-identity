using System.Text.Json;

namespace Identity.UnitTests.Infrastructure.IntegrationEvents;

public sealed class IntegrationEventLogEntryTests
{
    [Fact]
    public void Constructor_ShouldInitializeAllFields()
    {
        // Arrange
        var testEvent = new TestIntegrationEvent { Message = "Test message" };
        var transactionId = Guid.NewGuid();
        var beforeCreation = DateTime.UtcNow;

        // Act
        var entry = new IntegrationEventLogEntry(testEvent, transactionId);

        // Assert
        entry.EventId.Should().NotBeEmpty();
        entry.EventTypeName.Should().Be(typeof(TestIntegrationEvent).FullName);
        entry.Content.Should().NotBeNullOrEmpty();
        entry.CreationTime.Should().BeCloseTo(beforeCreation, TimeSpan.FromSeconds(1));
        entry.State.Should().Be(EventStateEnum.NotPublished);
        entry.TimesSent.Should().Be(0);
        entry.TransactionId.Should().Be(transactionId);
    }

    [Fact]
    public void Constructor_ShouldSerializeEventToJson()
    {
        // Arrange
        var testEvent = new TestIntegrationEvent { Message = "Test message", Value = 42 };
        var transactionId = Guid.NewGuid();

        // Act
        var entry = new IntegrationEventLogEntry(testEvent, transactionId);

        // Assert
        var deserializedEvent = JsonSerializer.Deserialize<TestIntegrationEvent>(entry.Content);
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.Message.Should().Be("Test message");
        deserializedEvent.Value.Should().Be(42);
    }

    [Fact]
    public void Constructor_ShouldSetStateToNotPublished()
    {
        // Arrange
        var testEvent = new TestIntegrationEvent();
        var transactionId = Guid.NewGuid();

        // Act
        var entry = new IntegrationEventLogEntry(testEvent, transactionId);

        // Assert
        entry.State.Should().Be(EventStateEnum.NotPublished);
    }

    [Fact]
    public void Constructor_ShouldSetTimesSentToZero()
    {
        // Arrange
        var testEvent = new TestIntegrationEvent();
        var transactionId = Guid.NewGuid();

        // Act
        var entry = new IntegrationEventLogEntry(testEvent, transactionId);

        // Assert
        entry.TimesSent.Should().Be(0);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueEventIds()
    {
        // Arrange
        var testEvent = new TestIntegrationEvent();
        var transactionId = Guid.NewGuid();

        // Act
        var entry1 = new IntegrationEventLogEntry(testEvent, transactionId);
        var entry2 = new IntegrationEventLogEntry(testEvent, transactionId);

        // Assert
        entry1.EventId.Should().NotBe(entry2.EventId);
    }

    [Fact]
    public void State_ShouldBeSettable()
    {
        // Arrange
        var testEvent = new TestIntegrationEvent();
        var transactionId = Guid.NewGuid();
        var entry = new IntegrationEventLogEntry(testEvent, transactionId);

        // Act
        entry.State = EventStateEnum.Published;

        // Assert
        entry.State.Should().Be(EventStateEnum.Published);
    }

    [Fact]
    public void TimesSent_ShouldBeSettable()
    {
        // Arrange
        var testEvent = new TestIntegrationEvent();
        var transactionId = Guid.NewGuid();
        var entry = new IntegrationEventLogEntry(testEvent, transactionId);

        // Act
        entry.TimesSent = 5;

        // Assert
        entry.TimesSent.Should().Be(5);
    }

    [Fact]
    public void Constructor_ShouldSetCorrectEventTypeName()
    {
        // Arrange
        var testEvent = new AnotherTestEvent { Data = "test" };
        var transactionId = Guid.NewGuid();

        // Act
        var entry = new IntegrationEventLogEntry(testEvent, transactionId);

        // Assert
        entry.EventTypeName.Should().Be(typeof(AnotherTestEvent).FullName);
    }

    [Fact]
    public void Constructor_ShouldHandleComplexObjects()
    {
        // Arrange
        var testEvent = new ComplexTestEvent
        {
            Id = 123,
            Name = "Test",
            Items = new List<string> { "item1", "item2" },
            Metadata = new Dictionary<string, object> { { "key1", "value1" } }
        };
        var transactionId = Guid.NewGuid();

        // Act
        var entry = new IntegrationEventLogEntry(testEvent, transactionId);

        // Assert
        entry.Content.Should().Contain("123");
        entry.Content.Should().Contain("Test");
        entry.Content.Should().Contain("item1");
        entry.Content.Should().Contain("item2");
    }

    private sealed class TestIntegrationEvent
    {
        public string Message { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    private sealed class AnotherTestEvent
    {
        public string Data { get; set; } = string.Empty;
    }

    private sealed class ComplexTestEvent
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Items { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
