namespace Identity.UnitTests.Infrastructure.Idempotency;

public sealed class ClientRequestTests
{
    [Fact]
    public void ClientRequest_ShouldSetProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "TestCommand";
        var time = DateTime.UtcNow;

        // Act
        var request = new ClientRequest
        {
            Id = id,
            Name = name,
            Time = time
        };

        // Assert
        request.Id.Should().Be(id);
        request.Name.Should().Be(name);
        request.Time.Should().Be(time);
    }

    [Fact]
    public void Id_ShouldBeSettable()
    {
        // Arrange
        var request = new ClientRequest { Name = "Test" };
        var newId = Guid.NewGuid();

        // Act
        request.Id = newId;

        // Assert
        request.Id.Should().Be(newId);
    }

    [Fact]
    public void Name_ShouldBeRequired()
    {
        // Arrange & Act
        var request = new ClientRequest { Name = "TestCommand" };

        // Assert
        request.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Time_ShouldBeSettable()
    {
        // Arrange
        var request = new ClientRequest { Name = "Test" };
        var newTime = DateTime.UtcNow.AddHours(-1);

        // Act
        request.Time = newTime;

        // Assert
        request.Time.Should().Be(newTime);
    }

    [Fact]
    public void ClientRequest_ShouldAllowDefaultGuid()
    {
        // Arrange & Act
        var request = new ClientRequest
        {
            Id = Guid.Empty,
            Name = "TestCommand",
            Time = DateTime.UtcNow
        };

        // Assert
        request.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ClientRequest_ShouldAllowDefaultDateTime()
    {
        // Arrange & Act
        var request = new ClientRequest
        {
            Id = Guid.NewGuid(),
            Name = "TestCommand",
            Time = default
        };

        // Assert
        request.Time.Should().Be(default);
    }
}
