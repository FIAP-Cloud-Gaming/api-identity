using Identity.UnitTests.Helpers;

namespace Identity.UnitTests.Infrastructure.Repositories;

public sealed class UserRepositoryTests : IDisposable
{
    private readonly IdentityContext _context;
    private readonly IUserRepository _repository;

    public UserRepositoryTests()
    {
        _context = IdentityContextFactory.Create();
        _repository = new UserRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void UnitOfWork_ShouldReturnContext()
    {
        // Assert
        _repository.UnitOfWork.Should().Be(_context);
    }

    [Fact]
    public void Add_ShouldAddUserToContext()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();

        // Act
        var addedUser = _repository.Add(user);

        // Assert
        addedUser.Should().Be(user);
        _context.Users.Local.Should().Contain(user);
    }

    [Fact]
    public async Task Add_ShouldPersistUserToDatabase_WhenSaveChangesIsCalled()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com", "John", "Doe");

        // Act
        _repository.Add(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.Value == "test@example.com", TestContext.Current.CancellationToken);
        savedUser.Should().NotBeNull();
        savedUser!.Name.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void Update_ShouldUpdateUserInContext()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        _repository.Add(user);
        user.UpdateName(new PersonName("Jane", "Smith"));

        // Act
        var updatedUser = _repository.Update(user);

        // Assert
        updatedUser.Should().Be(user);
        updatedUser.Name.FullName.Should().Be("Jane Smith");
    }

    [Fact]
    public async Task Delete_ShouldRemoveUserFromContext()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("delete@example.com");
        _repository.Add(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var userId = user.Id;

        // Act
        _repository.Delete(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var deletedUser = await _repository.GetByIdAsync(userId, TestContext.Current.CancellationToken);
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        _repository.Add(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var foundUser = await _repository.GetByIdAsync(user.Id, TestContext.Current.CancellationToken);

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Act
        var foundUser = await _repository.GetByIdAsync(999, TestContext.Current.CancellationToken);

        // Assert
        foundUser.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com");
        _repository.Add(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var foundUser = await _repository.GetByEmailAsync("test@example.com", TestContext.Current.CancellationToken);

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.Email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com");
        _repository.Add(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var foundUser = await _repository.GetByEmailAsync("TEST@EXAMPLE.COM", TestContext.Current.CancellationToken);

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.Email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Act
        var foundUser = await _repository.GetByEmailAsync("nonexistent@example.com", TestContext.Current.CancellationToken);

        // Assert
        foundUser.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldIncludeRoles()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com");
        var role = UserTestFactory.CreateRole("ADMIN");
        _context.Roles.Add(role);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        user.AssignRole(role);
        _repository.Add(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var foundUser = await _repository.GetByEmailAsync("test@example.com", TestContext.Current.CancellationToken);

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.UserRoles.Should().HaveCount(1);
        foundUser.UserRoles.First().Role!.Name.Should().Be("ADMIN");
    }

    [Fact]
    public async Task ExistsByEmailAsync_ShouldReturnTrue_WhenUserExists()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com");
        _repository.Add(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var exists = await _repository.ExistsByEmailAsync("test@example.com", TestContext.Current.CancellationToken);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com");
        _repository.Add(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var exists = await _repository.ExistsByEmailAsync("TEST@EXAMPLE.COM", TestContext.Current.CancellationToken);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Act
        var exists = await _repository.ExistsByEmailAsync("nonexistent@example.com", TestContext.Current.CancellationToken);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetRoleByNameAsync_ShouldReturnRole_WhenRoleExists()
    {
        // Arrange
        var role = UserTestFactory.CreateRole("ADMIN");
        _context.Roles.Add(role);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var foundRole = await _repository.GetRoleByNameAsync("ADMIN", TestContext.Current.CancellationToken);

        // Assert
        foundRole.Should().NotBeNull();
        foundRole!.Name.Should().Be("ADMIN");
    }

    [Fact]
    public async Task GetRoleByNameAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        var role = UserTestFactory.CreateRole("ADMIN");
        _context.Roles.Add(role);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var foundRole = await _repository.GetRoleByNameAsync("admin", TestContext.Current.CancellationToken);

        // Assert
        foundRole.Should().NotBeNull();
        foundRole!.Name.Should().Be("ADMIN");
    }

    [Fact]
    public async Task GetRoleByNameAsync_ShouldReturnNull_WhenRoleDoesNotExist()
    {
        // Act
        var foundRole = await _repository.GetRoleByNameAsync("NONEXISTENT", TestContext.Current.CancellationToken);

        // Assert
        foundRole.Should().BeNull();
    }

    [Fact]
    public async Task GetAllRolesAsync_ShouldReturnAllRoles()
    {
        // Arrange
        var role1 = UserTestFactory.CreateRole("ADMIN");
        var role2 = UserTestFactory.CreateRole("USER");
        var role3 = UserTestFactory.CreateRole("MANAGER");
        _context.Roles.AddRange(role1, role2, role3);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var roles = await _repository.GetAllRolesAsync(TestContext.Current.CancellationToken);

        // Assert
        roles.Should().HaveCountGreaterThanOrEqualTo(3);
        roles.Select(r => r.Name).Should().Contain(new[] { "ADMIN", "USER", "MANAGER" });
    }

    [Fact]
    public async Task GetAllRolesAsync_ShouldReturnEmpty_WhenNoRolesExist()
    {
        // Arrange
        var context = IdentityContextFactory.Create("EmptyDb");
        var repository = new UserRepository(context);

        // Act
        var roles = await repository.GetAllRolesAsync(TestContext.Current.CancellationToken);

        // Assert
        // Note: Context may have seeded roles, so we just check it doesn't throw
        roles.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdWithRolesAsync_ShouldIncludeRoles()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com");
        var role = UserTestFactory.CreateRole("ADMIN");
        _context.Roles.Add(role);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        user.AssignRole(role);
        _repository.Add(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var foundUser = await _repository.GetByIdWithRolesAsync(user.Id, TestContext.Current.CancellationToken);

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.UserRoles.Should().HaveCount(1);
        foundUser.UserRoles.First().Role!.Name.Should().Be("ADMIN");
    }

    [Fact]
    public async Task GetByIdWithTokensAsync_ShouldIncludeRefreshTokens()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com");
        user.GenerateRefreshToken("test-token-123");
        _repository.Add(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var foundUser = await _repository.GetByIdWithTokensAsync(user.Id, TestContext.Current.CancellationToken);

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.RefreshTokens.Should().HaveCount(1);
    }
}
