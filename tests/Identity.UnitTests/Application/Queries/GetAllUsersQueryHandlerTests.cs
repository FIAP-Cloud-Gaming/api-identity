using Identity.API.Application.Queries.GetAllUsers;
using Identity.UnitTests.Helpers;

namespace Identity.UnitTests.Application.Queries;

public sealed class GetAllUsersQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var context = IdentityContextFactory.Create(Guid.NewGuid().ToString());
        var handler = new GetAllUsersQueryHandler(context);

        for (int i = 1; i <= 15; i++)
        {
            var user = UserTestFactory.CreateValidUser(
                $"user{i}@example.com",
                $"User{i}",
                "Test",
                "hashed_password");
            context.Users.Add(user);
        }
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = new GetAllUsersQuery(Page: 2, PageSize: 5);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(5);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalRecords.Should().Be(15);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ReturnsOnlyMatchingUsers()
    {
        // Arrange
        var context = IdentityContextFactory.Create(Guid.NewGuid().ToString());
        var handler = new GetAllUsersQueryHandler(context);

        var activeUser = UserTestFactory.CreateValidUser(
            "active@example.com",
            "Active",
            "User",
            "hashed_password");

        var inactiveUser = UserTestFactory.CreateValidUser(
            "inactive@example.com",
            "Inactive",
            "User",
            "hashed_password");
        inactiveUser.Deactivate();

        context.Users.Add(activeUser);
        context.Users.Add(inactiveUser);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = new GetAllUsersQuery(Status: "Active");

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Data.Should().ContainSingle();
        result.Data[0].Email.Should().Be("active@example.com");
        result.Data[0].Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_WithRoleFilter_ReturnsOnlyUsersWithRole()
    {
        // Arrange
        var context = IdentityContextFactory.Create(Guid.NewGuid().ToString());
        var handler = new GetAllUsersQueryHandler(context);

        var adminRole = UserTestFactory.CreateRole("ADMIN", "Admin role", false);
        var userRole = UserTestFactory.CreateRole("USER", "User role", false);

        context.Roles.Add(adminRole);
        context.Roles.Add(userRole);

        var adminUser = UserTestFactory.CreateValidUser(
            "admin@example.com",
            "Admin",
            "User",
            "hashed_password");
        adminUser.AssignRole(adminRole);

        var normalUser = UserTestFactory.CreateValidUser(
            "user@example.com",
            "Normal",
            "User",
            "hashed_password");
        normalUser.AssignRole(userRole);

        context.Users.Add(adminUser);
        context.Users.Add(normalUser);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = new GetAllUsersQuery(Role: "ADMIN");

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Data.Should().ContainSingle();
        result.Data[0].Email.Should().Be("admin@example.com");
    }

    [Fact]
    public async Task Handle_WithRoleFilterCaseInsensitive_ReturnsCorrectUsers()
    {
        // Arrange
        var context = IdentityContextFactory.Create(Guid.NewGuid().ToString());
        var handler = new GetAllUsersQueryHandler(context);

        var adminRole = UserTestFactory.CreateRole("ADMIN", "Admin role", false);
        context.Roles.Add(adminRole);

        var adminUser = UserTestFactory.CreateValidUser(
            "admin@example.com",
            "Admin",
            "User",
            "hashed_password");
        adminUser.AssignRole(adminRole);

        context.Users.Add(adminUser);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = new GetAllUsersQuery(Role: "admin");

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Data.Should().ContainSingle();
        result.Data[0].Email.Should().Be("admin@example.com");
    }

    [Fact]
    public async Task Handle_NoFilters_ReturnsAllUsers()
    {
        // Arrange
        var context = IdentityContextFactory.Create(Guid.NewGuid().ToString());
        var handler = new GetAllUsersQueryHandler(context);

        for (int i = 1; i <= 5; i++)
        {
            var user = UserTestFactory.CreateValidUser(
                $"user{i}@example.com",
                $"User{i}",
                "Test",
                "hashed_password");
            context.Users.Add(user);
        }
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = new GetAllUsersQuery();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Data.Should().HaveCount(5);
        result.TotalRecords.Should().Be(5);
    }

    [Fact]
    public async Task Handle_OrdersByCreatedAtDescending()
    {
        // Arrange
        var context = IdentityContextFactory.Create(Guid.NewGuid().ToString());
        var handler = new GetAllUsersQueryHandler(context);

        var user1 = UserTestFactory.CreateValidUser("user1@example.com", "User1", "Test", "hash");
        await Task.Delay(10, TestContext.Current.CancellationToken);
        var user2 = UserTestFactory.CreateValidUser("user2@example.com", "User2", "Test", "hash");
        await Task.Delay(10, TestContext.Current.CancellationToken);
        var user3 = UserTestFactory.CreateValidUser("user3@example.com", "User3", "Test", "hash");

        context.Users.Add(user1);
        context.Users.Add(user2);
        context.Users.Add(user3);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = new GetAllUsersQuery(PageSize: 10);

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Data.Should().HaveCount(3);
        result.Data[0].Email.Should().Be("user3@example.com");
        result.Data[1].Email.Should().Be("user2@example.com");
        result.Data[2].Email.Should().Be("user1@example.com");
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyResult()
    {
        // Arrange
        var context = IdentityContextFactory.Create(Guid.NewGuid().ToString());
        var handler = new GetAllUsersQueryHandler(context);
        var query = new GetAllUsersQuery();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Data.Should().BeEmpty();
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_UserListViewModel_ContainsCorrectProperties()
    {
        // Arrange
        var context = IdentityContextFactory.Create(Guid.NewGuid().ToString());
        var handler = new GetAllUsersQueryHandler(context);

        var user = UserTestFactory.CreateValidUser(
            "user@example.com",
            "John",
            "Doe",
            "hashed_password");

        context.Users.Add(user);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var query = new GetAllUsersQuery();

        // Act
        var result = await handler.Handle(query, TestContext.Current.CancellationToken);

        // Assert
        result.Data.Should().ContainSingle();
        var userViewModel = result.Data[0];
        userViewModel.Id.Should().Be(user.Id);
        userViewModel.Email.Should().Be("user@example.com");
        userViewModel.FullName.Should().Be("John Doe");
        userViewModel.Status.Should().Be("Active");
        userViewModel.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
