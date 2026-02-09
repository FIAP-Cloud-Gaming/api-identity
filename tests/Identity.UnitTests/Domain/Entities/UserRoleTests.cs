using Identity.UnitTests.Helpers;

namespace Identity.UnitTests.Domain.Entities;

public sealed class UserRoleTests
{
    [Fact]
    public void AssignRole_ShouldCreateUserRoleWithCorrectProperties()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var role = UserTestFactory.CreateRole("USER");

        // Simulate having IDs (as if they were persisted)
        typeof(Role).GetProperty("Id")!.SetValue(role, 10);

        // Act
        user.AssignRole(role);

        // Assert
        var userRole = user.UserRoles.Should().ContainSingle().Subject;
        userRole.UserId.Should().Be(user.Id);
        userRole.RoleId.Should().Be(role.Id);
        userRole.AssignedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AssignRole_ShouldSetAssignedAtTimestamp()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var role = UserTestFactory.CreateRole("USER");
        typeof(Role).GetProperty("Id")!.SetValue(role, 10);

        // Act
        user.AssignRole(role);

        // Assert
        var userRole = user.UserRoles.First();
        userRole.AssignedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AssignRole_ShouldSetUserId()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var role = UserTestFactory.CreateRole("USER");
        typeof(Role).GetProperty("Id")!.SetValue(role, 10);

        // Act
        user.AssignRole(role);

        // Assert
        var userRole = user.UserRoles.First();
        userRole.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void AssignRole_ShouldSetRoleId()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var role = UserTestFactory.CreateRole("ADMIN");
        typeof(Role).GetProperty("Id")!.SetValue(role, 42);

        // Act
        user.AssignRole(role);

        // Assert
        var userRole = user.UserRoles.First();
        userRole.RoleId.Should().Be(42);
    }

    [Fact]
    public void UserRole_User_ShouldBeNullByDefault()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var role = UserTestFactory.CreateRole("USER");
        typeof(Role).GetProperty("Id")!.SetValue(role, 10);

        // Act
        user.AssignRole(role);

        // Assert
        var userRole = user.UserRoles.First();
        userRole.User.Should().BeNull();
    }

    [Fact]
    public void UserRole_Role_ShouldBeNullByDefault()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var role = UserTestFactory.CreateRole("USER");
        typeof(Role).GetProperty("Id")!.SetValue(role, 10);

        // Act
        user.AssignRole(role);

        // Assert
        var userRole = user.UserRoles.First();
        userRole.Role.Should().BeNull();
    }

    [Fact]
    public void AssignRole_MultipleTimes_ShouldCreateMultipleUserRoles()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var role1 = UserTestFactory.CreateRole("USER");
        var role2 = UserTestFactory.CreateRole("ADMIN");
        typeof(Role).GetProperty("Id")!.SetValue(role1, 1);
        typeof(Role).GetProperty("Id")!.SetValue(role2, 2);

        // Act
        user.AssignRole(role1);
        user.AssignRole(role2);

        // Assert
        user.UserRoles.Should().HaveCount(2);
        user.UserRoles.Should().Contain(ur => ur.RoleId == 1);
        user.UserRoles.Should().Contain(ur => ur.RoleId == 2);
    }

    [Fact]
    public void AssignRole_WithSameRole_ShouldNotCreateDuplicate()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var role = UserTestFactory.CreateRole("USER");
        typeof(Role).GetProperty("Id")!.SetValue(role, 10);

        // Act
        user.AssignRole(role);
        user.AssignRole(role);

        // Assert
        user.UserRoles.Should().HaveCount(1);
    }
}
