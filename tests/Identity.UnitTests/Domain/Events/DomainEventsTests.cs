using Identity.UnitTests.Helpers;

namespace Identity.UnitTests.Domain.Events;

public sealed class DomainEventsTests
{
    [Fact]
    public void UserRegisteredDomainEvent_ShouldImplementINotification()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();

        // Act
        var domainEvent = new UserRegisteredDomainEvent(user);

        // Assert
        domainEvent.Should().BeAssignableTo<INotification>();
    }

    [Fact]
    public void UserRegisteredDomainEvent_ShouldHaveCorrectProperties()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        typeof(User).GetProperty("Id")!.SetValue(user, 42);

        // Act
        var domainEvent = new UserRegisteredDomainEvent(user);

        // Assert
        domainEvent.User.Should().Be(user);
        domainEvent.UserId.Should().Be(42);
        domainEvent.Email.Should().Be(user.Email.Value);
        domainEvent.FullName.Should().Be(user.Name.FullName);
    }

    [Fact]
    public void PasswordChangedDomainEvent_ShouldImplementINotification()
    {
        // Arrange & Act
        var domainEvent = new PasswordChangedDomainEvent(1, "test@example.com");

        // Assert
        domainEvent.Should().BeAssignableTo<INotification>();
    }

    [Fact]
    public void PasswordChangedDomainEvent_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var domainEvent = new PasswordChangedDomainEvent(42, "user@example.com");

        // Assert
        domainEvent.UserId.Should().Be(42);
        domainEvent.Email.Should().Be("user@example.com");
    }

    [Fact]
    public void UserActivatedDomainEvent_ShouldImplementINotification()
    {
        // Arrange & Act
        var domainEvent = new UserActivatedDomainEvent(1, "test@example.com");

        // Assert
        domainEvent.Should().BeAssignableTo<INotification>();
    }

    [Fact]
    public void UserActivatedDomainEvent_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var domainEvent = new UserActivatedDomainEvent(42, "user@example.com");

        // Assert
        domainEvent.UserId.Should().Be(42);
        domainEvent.Email.Should().Be("user@example.com");
    }

    [Fact]
    public void UserDeactivatedDomainEvent_ShouldImplementINotification()
    {
        // Arrange & Act
        var domainEvent = new UserDeactivatedDomainEvent(1, "test@example.com");

        // Assert
        domainEvent.Should().BeAssignableTo<INotification>();
    }

    [Fact]
    public void UserDeactivatedDomainEvent_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var domainEvent = new UserDeactivatedDomainEvent(42, "user@example.com");

        // Assert
        domainEvent.UserId.Should().Be(42);
        domainEvent.Email.Should().Be("user@example.com");
    }

    [Fact]
    public void RoleAssignedDomainEvent_ShouldImplementINotification()
    {
        // Arrange & Act
        var domainEvent = new RoleAssignedDomainEvent(1, "USER");

        // Assert
        domainEvent.Should().BeAssignableTo<INotification>();
    }

    [Fact]
    public void RoleAssignedDomainEvent_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var domainEvent = new RoleAssignedDomainEvent(42, "ADMIN");

        // Assert
        domainEvent.UserId.Should().Be(42);
        domainEvent.RoleName.Should().Be("ADMIN");
    }

    [Fact]
    public void RoleRemovedDomainEvent_ShouldImplementINotification()
    {
        // Arrange & Act
        var domainEvent = new RoleRemovedDomainEvent(1, "USER");

        // Assert
        domainEvent.Should().BeAssignableTo<INotification>();
    }

    [Fact]
    public void RoleRemovedDomainEvent_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var domainEvent = new RoleRemovedDomainEvent(42, "MANAGER");

        // Assert
        domainEvent.UserId.Should().Be(42);
        domainEvent.RoleName.Should().Be("MANAGER");
    }

    [Fact]
    public void UserLoggedInDomainEvent_ShouldImplementINotification()
    {
        // Arrange
        var loginTime = DateTime.UtcNow;

        // Act
        var domainEvent = new UserLoggedInDomainEvent(1, "test@example.com", loginTime);

        // Assert
        domainEvent.Should().BeAssignableTo<INotification>();
    }

    [Fact]
    public void UserLoggedInDomainEvent_ShouldHaveCorrectProperties()
    {
        // Arrange
        var loginTime = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var domainEvent = new UserLoggedInDomainEvent(42, "user@example.com", loginTime);

        // Assert
        domainEvent.UserId.Should().Be(42);
        domainEvent.Email.Should().Be("user@example.com");
        domainEvent.LoginAt.Should().Be(loginTime);
    }

    [Fact]
    public void RefreshTokenGeneratedDomainEvent_ShouldImplementINotification()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var domainEvent = new RefreshTokenGeneratedDomainEvent(1, expiresAt);

        // Assert
        domainEvent.Should().BeAssignableTo<INotification>();
    }

    [Fact]
    public void RefreshTokenGeneratedDomainEvent_ShouldHaveCorrectProperties()
    {
        // Arrange
        var expiresAt = new DateTime(2026, 1, 22, 10, 30, 0, DateTimeKind.Utc);

        // Act
        var domainEvent = new RefreshTokenGeneratedDomainEvent(42, expiresAt);

        // Assert
        domainEvent.UserId.Should().Be(42);
        domainEvent.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void RefreshTokenRevokedDomainEvent_ShouldImplementINotification()
    {
        // Arrange & Act
        var domainEvent = new RefreshTokenRevokedDomainEvent(1, "User logout");

        // Assert
        domainEvent.Should().BeAssignableTo<INotification>();
    }

    [Fact]
    public void RefreshTokenRevokedDomainEvent_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var domainEvent = new RefreshTokenRevokedDomainEvent(42, "Security breach");

        // Assert
        domainEvent.UserId.Should().Be(42);
        domainEvent.Reason.Should().Be("Security breach");
    }

    [Fact]
    public void DomainEvents_ShouldBeRecords()
    {
        // Assert
        typeof(UserRegisteredDomainEvent).Should().NotBeSealed();
        typeof(PasswordChangedDomainEvent).Should().NotBeSealed();
        typeof(UserActivatedDomainEvent).Should().NotBeSealed();
        typeof(UserDeactivatedDomainEvent).Should().NotBeSealed();
        typeof(RoleAssignedDomainEvent).Should().NotBeSealed();
        typeof(RoleRemovedDomainEvent).Should().NotBeSealed();
        typeof(UserLoggedInDomainEvent).Should().NotBeSealed();
        typeof(RefreshTokenGeneratedDomainEvent).Should().NotBeSealed();
        typeof(RefreshTokenRevokedDomainEvent).Should().NotBeSealed();
    }

    [Fact]
    public void DomainEvents_RecordEquality_ShouldWorkCorrectly()
    {
        // Arrange
        var event1 = new PasswordChangedDomainEvent(1, "test@example.com");
        var event2 = new PasswordChangedDomainEvent(1, "test@example.com");
        var event3 = new PasswordChangedDomainEvent(2, "test@example.com");

        // Assert
        event1.Should().Be(event2);
        event1.Should().NotBe(event3);
    }

    [Fact]
    public void UserRegisteredDomainEvent_WithMultipleProperties_ShouldExposeAllCorrectly()
    {
        // Arrange
        var email = new Email("john.doe@example.com");
        var password = Password.FromHash("hash123");
        var name = new PersonName("John", "Doe");
        var user = User.Create(email, password, name);
        typeof(User).GetProperty("Id")!.SetValue(user, 100);

        // Act
        var domainEvent = new UserRegisteredDomainEvent(user);

        // Assert
        domainEvent.UserId.Should().Be(100);
        domainEvent.Email.Should().Be("john.doe@example.com");
        domainEvent.FullName.Should().Be("John Doe");
        domainEvent.User.Should().BeSameAs(user);
    }
}
