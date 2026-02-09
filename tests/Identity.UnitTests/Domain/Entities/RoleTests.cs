using Identity.UnitTests.Helpers;

namespace Identity.UnitTests.Domain.Entities;

public sealed class RoleTests
{
    [Fact]
    public void Role_WithValidName_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var role = new Role("USER");

        // Assert
        role.Name.Should().Be("USER");
        role.Description.Should().BeNull();
        role.IsSystem.Should().BeFalse();
    }

    [Fact]
    public void Role_WithDescriptionAndIsSystem_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var role = new Role("ADMIN", "Administrator role", isSystem: true);

        // Assert
        role.Name.Should().Be("ADMIN");
        role.Description.Should().Be("Administrator role");
        role.IsSystem.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Role_WithNullOrWhitespaceName_ShouldThrowIdentityDomainException(string? invalidName)
    {
        // Arrange & Act
        var act = () => new Role(invalidName!);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("Role name is required");
    }

    [Theory]
    [InlineData("user", "USER")]
    [InlineData("admin", "ADMIN")]
    [InlineData("Manager", "MANAGER")]
    [InlineData("guest", "GUEST")]
    public void Role_ShouldNormalizeNameToUppercase(string input, string expected)
    {
        // Arrange & Act
        var role = new Role(input);

        // Assert
        role.Name.Should().Be(expected);
    }

    [Fact]
    public void UpdateDescription_ShouldUpdateDescription()
    {
        // Arrange
        var role = new Role("USER", "Original description");

        // Act
        role.UpdateDescription("Updated description");

        // Assert
        role.Description.Should().Be("Updated description");
    }

    [Fact]
    public void UpdateDescription_WithNull_ShouldSetDescriptionToNull()
    {
        // Arrange
        var role = new Role("USER", "Original description");

        // Act
        role.UpdateDescription(null);

        // Assert
        role.Description.Should().BeNull();
    }

    [Fact]
    public void Role_Roles_Admin_ShouldHaveCorrectValue()
    {
        // Act & Assert
        Role.Roles.Admin.Should().Be("ADMIN");
    }

    [Fact]
    public void Role_Roles_User_ShouldHaveCorrectValue()
    {
        // Act & Assert
        Role.Roles.User.Should().Be("USER");
    }

    [Fact]
    public void Role_Roles_Manager_ShouldHaveCorrectValue()
    {
        // Act & Assert
        Role.Roles.Manager.Should().Be("MANAGER");
    }

    [Fact]
    public void Role_UserRoles_ShouldInitializeAsEmpty()
    {
        // Arrange & Act
        var role = new Role("USER");

        // Assert
        role.UserRoles.Should().NotBeNull();
        role.UserRoles.Should().BeEmpty();
    }

    [Fact]
    public void Role_WithOnlyName_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var role = new Role("GUEST");

        // Assert
        role.Name.Should().Be("GUEST");
        role.Description.Should().BeNull();
        role.IsSystem.Should().BeFalse();
    }

    [Fact]
    public void CreateRole_UsingTestFactory_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var role = UserTestFactory.CreateRole("ADMIN", "Admin role", isSystem: true);

        // Assert
        role.Name.Should().Be("ADMIN");
        role.Description.Should().Be("Admin role");
        role.IsSystem.Should().BeTrue();
    }

    [Fact]
    public void Role_IsSystem_ShouldBeFalseByDefault()
    {
        // Arrange & Act
        var role = new Role("USER");

        // Assert
        role.IsSystem.Should().BeFalse();
    }

    [Fact]
    public void Role_WithMultipleUpdates_ShouldMaintainLastDescription()
    {
        // Arrange
        var role = new Role("USER", "First");

        // Act
        role.UpdateDescription("Second");
        role.UpdateDescription("Third");

        // Assert
        role.Description.Should().Be("Third");
    }
}
