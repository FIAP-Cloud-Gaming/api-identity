using Identity.API.Application.Queries.GetUserById;
using Identity.UnitTests.Helpers;

namespace Identity.UnitTests.Application.Queries;

public sealed class GetUserByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_UserExists_ReturnsUserViewModel()
    {
        // Arrange
        var context = IdentityContextFactory.Create();
        var handler = new GetUserByIdQueryHandler(context);

        var user = UserTestFactory.CreateValidUser(
            "user@example.com",
            "John",
            "Doe",
            "hashed_password");

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var query = new GetUserByIdQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be("user@example.com");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.FullName.Should().Be("John Doe");
        result.Status.Should().Be("Active");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UserNotExists_ReturnsNull()
    {
        // Arrange
        var context = IdentityContextFactory.Create();
        var handler = new GetUserByIdQueryHandler(context);
        var query = new GetUserByIdQuery(999);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UserWithRoles_IncludesRolesInResult()
    {
        // Arrange
        var context = IdentityContextFactory.Create();
        var handler = new GetUserByIdQueryHandler(context);

        var user = UserTestFactory.CreateValidUser(
            "user@example.com",
            "John",
            "Doe",
            "hashed_password");

        var userRole = UserTestFactory.CreateRole("USER", "User role", false);
        var adminRole = UserTestFactory.CreateRole("ADMIN", "Admin role", false);

        context.Roles.Add(userRole);
        context.Roles.Add(adminRole);
        user.AssignRole(userRole);
        user.AssignRole(adminRole);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var query = new GetUserByIdQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Roles.Should().HaveCount(2);
        result.Roles.Should().Contain("USER");
        result.Roles.Should().Contain("ADMIN");
    }

    [Fact]
    public async Task Handle_UserWithoutRoles_ReturnsEmptyRolesList()
    {
        // Arrange
        var context = IdentityContextFactory.Create();
        var handler = new GetUserByIdQueryHandler(context);

        var user = UserTestFactory.CreateValidUser(
            "user@example.com",
            "John",
            "Doe",
            "hashed_password");

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var query = new GetUserByIdQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Roles.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_UserWithLastLogin_IncludesLastLoginAt()
    {
        // Arrange
        var context = IdentityContextFactory.Create();
        var handler = new GetUserByIdQueryHandler(context);

        var user = UserTestFactory.CreateValidUser(
            "user@example.com",
            "John",
            "Doe",
            "hashed_password");

        user.RecordLogin();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var query = new GetUserByIdQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.LastLoginAt.Should().NotBeNull();
        result.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
