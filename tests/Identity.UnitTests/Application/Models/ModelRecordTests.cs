using Identity.API.Application.Models;
using Identity.API.Application.Commands.RegisterUser;

namespace Identity.UnitTests.Application.Models;

public sealed class ModelRecordTests
{
    [Fact]
    public void LoginResponse_StoresAllProperties()
    {
        // Arrange
        var userId = 1;
        var email = "user@example.com";
        var fullName = "John Doe";
        var accessToken = "access_token_value";
        var refreshToken = "refresh_token_value";
        var expiration = DateTime.UtcNow.AddHours(1);
        var roles = new List<string> { "USER", "ADMIN" };

        // Act
        var response = new LoginResponse(
            userId,
            email,
            fullName,
            accessToken,
            refreshToken,
            expiration,
            roles);

        // Assert
        response.UserId.Should().Be(userId);
        response.Email.Should().Be(email);
        response.FullName.Should().Be(fullName);
        response.AccessToken.Should().Be(accessToken);
        response.RefreshToken.Should().Be(refreshToken);
        response.AccessTokenExpiration.Should().Be(expiration);
        response.Roles.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public void UserViewModel_StoresAllProperties()
    {
        // Arrange
        var id = 1;
        var email = "user@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var fullName = "John Doe";
        var status = "Active";
        var createdAt = DateTime.UtcNow;
        var lastLoginAt = DateTime.UtcNow.AddHours(-1);
        var roles = new List<string> { "USER" };

        // Act
        var viewModel = new UserViewModel(
            id,
            email,
            firstName,
            lastName,
            fullName,
            status,
            createdAt,
            lastLoginAt,
            roles);

        // Assert
        viewModel.Id.Should().Be(id);
        viewModel.Email.Should().Be(email);
        viewModel.FirstName.Should().Be(firstName);
        viewModel.LastName.Should().Be(lastName);
        viewModel.FullName.Should().Be(fullName);
        viewModel.Status.Should().Be(status);
        viewModel.CreatedAt.Should().Be(createdAt);
        viewModel.LastLoginAt.Should().Be(lastLoginAt);
        viewModel.Roles.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public void UserListViewModel_StoresAllProperties()
    {
        // Arrange
        var id = 1;
        var email = "user@example.com";
        var fullName = "John Doe";
        var status = "Active";
        var createdAt = DateTime.UtcNow;

        // Act
        var viewModel = new UserListViewModel(
            id,
            email,
            fullName,
            status,
            createdAt);

        // Assert
        viewModel.Id.Should().Be(id);
        viewModel.Email.Should().Be(email);
        viewModel.FullName.Should().Be(fullName);
        viewModel.Status.Should().Be(status);
        viewModel.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void TokenValidationResponse_StoresAllProperties()
    {
        // Arrange
        var userId = "1";
        var userName = "John Doe";
        var email = "user@example.com";
        var roles = new List<string> { "USER", "ADMIN" };
        var isAuthenticated = true;

        // Act
        var response = new TokenValidationResponse(
            userId,
            userName,
            email,
            roles,
            isAuthenticated);

        // Assert
        response.UserId.Should().Be(userId);
        response.UserName.Should().Be(userName);
        response.Email.Should().Be(email);
        response.Roles.Should().BeEquivalentTo(roles);
        response.IsAuthenticated.Should().Be(isAuthenticated);
    }

    [Fact]
    public void RegisterUserResult_StoresAllProperties()
    {
        // Arrange
        var userId = 1;
        var email = "user@example.com";
        var fullName = "John Doe";

        // Act
        var result = new RegisterUserResult(userId, email, fullName);

        // Assert
        result.UserId.Should().Be(userId);
        result.Email.Should().Be(email);
        result.FullName.Should().Be(fullName);
    }

    [Fact]
    public void LoginResponse_WithEmptyRoles_AllowsEmptyList()
    {
        // Arrange
        var roles = new List<string>();

        // Act
        var response = new LoginResponse(
            1,
            "user@example.com",
            "John Doe",
            "token",
            "refresh",
            DateTime.UtcNow,
            roles);

        // Assert
        response.Roles.Should().BeEmpty();
    }

    [Fact]
    public void UserViewModel_WithNullLastLoginAt_AllowsNull()
    {
        // Arrange & Act
        var viewModel = new UserViewModel(
            1,
            "user@example.com",
            "John",
            "Doe",
            "John Doe",
            "Active",
            DateTime.UtcNow,
            null,
            new List<string>());

        // Assert
        viewModel.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public void TokenValidationResponse_WithNullValues_AllowsNulls()
    {
        // Arrange & Act
        var response = new TokenValidationResponse(
            null,
            null,
            null,
            Enumerable.Empty<string>(),
            false);

        // Assert
        response.UserId.Should().BeNull();
        response.UserName.Should().BeNull();
        response.Email.Should().BeNull();
        response.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void Records_AreImmutable()
    {
        // Arrange
        var loginResponse = new LoginResponse(1, "email", "name", "token", "refresh", DateTime.UtcNow, new List<string>());
        var userViewModel = new UserViewModel(1, "email", "first", "last", "full", "status", DateTime.UtcNow, null, new List<string>());
        var userListViewModel = new UserListViewModel(1, "email", "full", "status", DateTime.UtcNow);
        var tokenValidation = new TokenValidationResponse("1", "name", "email", new List<string>(), true);
        var registerResult = new RegisterUserResult(1, "email", "name");

        // Assert - Records should have init-only setters (IsInitOnly returns true)
        var userIdProp = loginResponse.GetType().GetProperty("UserId")!;
        var setMethod = userIdProp.GetSetMethod(true);
        setMethod.Should().NotBeNull();
        setMethod!.ReturnParameter.GetRequiredCustomModifiers()
            .Any(x => x.Name == "IsExternalInit")
            .Should().BeTrue();
    }

    [Fact]
    public void Records_EqualityWorks()
    {
        // Arrange
        var result1 = new RegisterUserResult(1, "user@example.com", "John Doe");
        var result2 = new RegisterUserResult(1, "user@example.com", "John Doe");
        var result3 = new RegisterUserResult(2, "other@example.com", "Jane Doe");

        // Assert
        result1.Should().Be(result2);
        result1.Should().NotBe(result3);
        (result1 == result2).Should().BeTrue();
        (result1 == result3).Should().BeFalse();
    }

    [Fact]
    public void UserViewModel_WithMultipleRoles_StoresAllRoles()
    {
        // Arrange
        var roles = new List<string> { "USER", "ADMIN", "MODERATOR" };

        // Act
        var viewModel = new UserViewModel(
            1,
            "user@example.com",
            "John",
            "Doe",
            "John Doe",
            "Active",
            DateTime.UtcNow,
            null,
            roles);

        // Assert
        viewModel.Roles.Should().HaveCount(3);
        viewModel.Roles.Should().Contain("USER");
        viewModel.Roles.Should().Contain("ADMIN");
        viewModel.Roles.Should().Contain("MODERATOR");
    }
}
