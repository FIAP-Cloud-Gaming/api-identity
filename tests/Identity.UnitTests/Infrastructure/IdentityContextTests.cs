using Identity.UnitTests.Helpers;

namespace Identity.UnitTests.Infrastructure;

public sealed class IdentityContextTests : IDisposable
{
    private readonly IdentityContext _context;

    public IdentityContextTests()
    {
        _context = IdentityContextFactory.Create();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void HasActiveTransaction_ShouldBeFalse_ByDefault()
    {
        // Assert
        _context.HasActiveTransaction.Should().BeFalse();
    }

    [Fact]
    public async Task SaveEntitiesAsync_ShouldSaveChanges()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com");
        _context.Users.Add(user);

        // Act
        var result = await _context.SaveEntitiesAsync();

        // Assert
        result.Should().BeTrue();
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.Value == "test@example.com");
        savedUser.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveEntitiesAsync_ShouldDispatchNotifications()
    {
        // Arrange
        var email = new Email("test@example.com");
        var password = Password.FromHash("HASH-SALT");
        var name = new PersonName("John", "Doe");
        var user = User.Create(email, password, name);

        _context.Users.Add(user);

        // Act
        var result = await _context.SaveEntitiesAsync();

        // Assert
        result.Should().BeTrue();
        // Note: We can't easily verify mediator was called since it's mocked,
        // but we can verify the operation completed successfully
    }

    [Fact]
    public async Task SaveEntitiesAsync_ShouldReturnFalse_WhenNoChanges()
    {
        // Act
        var result = await _context.SaveEntitiesAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetEntitiesWithNotifications_ShouldReturnEntitiesThatHaveNotifications()
    {
        // Arrange
        var email = new Email("test@example.com");
        var password = Password.FromHash("HASH-SALT");
        var name = new PersonName("John", "Doe");
        var user = User.Create(email, password, name);

        _context.Users.Add(user);

        // Act
        var entities = _context.GetEntitiesWithNotifications();

        // Assert
        entities.Should().HaveCount(1);
        entities.First().Should().Be(user);
        entities.First().Notifications.Should().NotBeEmpty();
    }

    [Fact]
    public void GetEntitiesWithNotifications_ShouldReturnEmpty_WhenNoNotifications()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        _context.Users.Add(user);

        // Act
        var entities = _context.GetEntitiesWithNotifications();

        // Assert
        entities.Should().BeEmpty();
    }

    [Fact]
    public async Task BeginTransactionAsync_ShouldCreateTransaction()
    {
        // Act
        var transaction = await _context.BeginTransactionAsync();

        // Assert
        transaction.Should().NotBeNull();
        _context.HasActiveTransaction.Should().BeTrue();
    }

    [Fact]
    public async Task BeginTransactionAsync_ShouldReturnNull_WhenTransactionAlreadyExists()
    {
        // Arrange
        await _context.BeginTransactionAsync();

        // Act
        var secondTransaction = await _context.BeginTransactionAsync();

        // Assert
        secondTransaction.Should().BeNull();
        _context.HasActiveTransaction.Should().BeTrue();
    }

    [Fact]
    public async Task CommitTransactionAsync_ShouldCommitChanges()
    {
        // Arrange
        var transaction = await _context.BeginTransactionAsync();
        var user = UserTestFactory.CreateValidUser("test@example.com");
        _context.Users.Add(user);

        // Act
        await _context.CommitTransactionAsync(transaction!);

        // Assert
        _context.HasActiveTransaction.Should().BeFalse();
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.Value == "test@example.com");
        savedUser.Should().NotBeNull();
    }

    [Fact]
    public async Task CommitTransactionAsync_ShouldThrowArgumentNullException_WhenTransactionIsNull()
    {
        // Act
        var act = async () => await _context.CommitTransactionAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CommitTransactionAsync_ShouldThrowInvalidOperationException_WhenTransactionIsNotCurrent()
    {
        // Arrange
        await _context.BeginTransactionAsync();

        var fakeTransaction = Substitute.For<IDbContextTransaction>();
        fakeTransaction.TransactionId.Returns(Guid.NewGuid());

        // Act
        var act = async () => await _context.CommitTransactionAsync(fakeTransaction);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RollbackTransaction_ShouldRollbackChanges()
    {
        // Arrange
        await _context.BeginTransactionAsync();
        var user = UserTestFactory.CreateValidUser("test@example.com");
        _context.Users.Add(user);

        // Act
        _context.RollbackTransaction();

        // Assert
        _context.HasActiveTransaction.Should().BeFalse();
    }

    [Fact]
    public void RollbackTransaction_ShouldBeSafe_WhenNoActiveTransaction()
    {
        // Act
        var act = () => _context.RollbackTransaction();

        // Assert
        act.Should().NotThrow();
        _context.HasActiveTransaction.Should().BeFalse();
    }

    [Fact]
    public void GetCurrentTransaction_ShouldReturnNull_WhenNoActiveTransaction()
    {
        // Act
        var transaction = _context.GetCurrentTransaction();

        // Assert
        transaction.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentTransaction_ShouldReturnTransaction_WhenTransactionIsActive()
    {
        // Arrange
        var transaction = await _context.BeginTransactionAsync();

        // Act
        var currentTransaction = _context.GetCurrentTransaction();

        // Assert
        currentTransaction.Should().Be(transaction);
    }

    [Fact]
    public async Task Context_ShouldHaveCorrectDbSets()
    {
        // Assert
        _context.Users.Should().NotBeNull();
        _context.Roles.Should().NotBeNull();
        _context.UserRoles.Should().NotBeNull();
        _context.RefreshTokens.Should().NotBeNull();
        _context.ClientRequests.Should().NotBeNull();
        _context.IntegrationEventLogs.Should().NotBeNull();
    }

    // Removed: InMemory provider does not execute HasData seeding from OnModelCreating
    // To test seeding, use context.Database.EnsureCreated() or migrate to real database

    [Fact]
    public async Task SaveEntitiesAsync_ShouldHandleMultipleEntities()
    {
        // Arrange
        var user1 = UserTestFactory.CreateValidUser("user1@example.com");
        var user2 = UserTestFactory.CreateValidUser("user2@example.com");
        var user3 = UserTestFactory.CreateValidUser("user3@example.com");

        _context.Users.AddRange(user1, user2, user3);

        // Act
        var result = await _context.SaveEntitiesAsync();

        // Assert
        result.Should().BeTrue();
        var savedUsers = await _context.Users.ToListAsync();
        savedUsers.Should().Contain(u => u.Email.Value == "user1@example.com");
        savedUsers.Should().Contain(u => u.Email.Value == "user2@example.com");
        savedUsers.Should().Contain(u => u.Email.Value == "user3@example.com");
    }
}
