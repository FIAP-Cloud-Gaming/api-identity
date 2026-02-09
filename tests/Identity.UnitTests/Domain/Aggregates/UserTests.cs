using Identity.UnitTests.Helpers;

namespace Identity.UnitTests.Domain.Aggregates;

public sealed class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var email = new Email("test@example.com");
        var password = Password.FromHash("hash123");
        var name = new PersonName("John", "Doe");

        // Act
        var user = User.Create(email, password, name);

        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(password);
        user.Name.Should().Be(name);
        user.Status.Should().Be(UserStatus.Active);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldSetStatusToActive()
    {
        // Arrange
        var email = new Email("test@example.com");
        var password = Password.FromHash("hash123");
        var name = new PersonName("John", "Doe");

        // Act
        var user = User.Create(email, password, name);

        // Assert
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Create_ShouldRaiseUserRegisteredDomainEvent()
    {
        // Arrange
        var email = new Email("test@example.com");
        var password = Password.FromHash("hash123");
        var name = new PersonName("John", "Doe");

        // Act
        var user = User.Create(email, password, name);

        // Assert
        user.Notifications.Should().ContainSingle()
            .Which.Should().BeOfType<UserRegisteredDomainEvent>()
            .Which.User.Should().Be(user);
    }

    [Fact]
    public void ChangePassword_WhenActive_ShouldUpdatePassword()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var newPassword = Password.FromHash("newHash456");

        // Act
        user.ChangePassword(newPassword);

        // Assert
        user.PasswordHash.Should().Be(newPassword);
    }

    [Fact]
    public void ChangePassword_WhenActive_ShouldRaisePasswordChangedDomainEvent()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var newPassword = Password.FromHash("newHash456");

        // Act
        user.ChangePassword(newPassword);

        // Assert
        user.Notifications.Should().ContainSingle()
            .Which.Should().BeOfType<PasswordChangedDomainEvent>();
    }

    [Theory]
    [InlineData(UserStatus.Inactive)]
    [InlineData(UserStatus.Suspended)]
    [InlineData(UserStatus.PendingVerification)]
    public void ChangePassword_WhenNotActive_ShouldThrowIdentityDomainException(UserStatus status)
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        typeof(User).GetProperty("Status")!.SetValue(user, status);
        var newPassword = Password.FromHash("newHash456");

        // Act
        var act = () => user.ChangePassword(newPassword);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("Cannot change password for inactive user");
    }

    [Fact]
    public void UpdateName_ShouldUpdateUserName()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var newName = new PersonName("Jane", "Smith");

        // Act
        user.UpdateName(newName);

        // Assert
        user.Name.Should().Be(newName);
        user.Name.FirstName.Should().Be("Jane");
        user.Name.LastName.Should().Be("Smith");
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivateUser()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        user.Deactivate();
        user.ClearNotification();

        // Act
        user.Activate();

        // Assert
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldRaiseUserActivatedDomainEvent()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        user.Deactivate();
        user.ClearNotification();

        // Act
        user.Activate();

        // Assert
        user.Notifications.Should().ContainSingle()
            .Which.Should().BeOfType<UserActivatedDomainEvent>();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldBeIdempotent()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();

        // Act
        user.Activate();

        // Assert
        user.Status.Should().Be(UserStatus.Active);
        user.Notifications.Should().BeEmpty();
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateUser()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();

        // Act
        user.Deactivate();

        // Assert
        user.Status.Should().Be(UserStatus.Inactive);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldRaiseUserDeactivatedDomainEvent()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();

        // Act
        user.Deactivate();

        // Assert
        user.Notifications.Should().ContainSingle()
            .Which.Should().BeOfType<UserDeactivatedDomainEvent>();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldBeIdempotent()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        user.Deactivate();
        user.ClearNotification();

        // Act
        user.Deactivate();

        // Assert
        user.Status.Should().Be(UserStatus.Inactive);
        user.Notifications.Should().BeEmpty();
    }

    [Fact]
    public void Deactivate_ShouldRevokeAllRefreshTokens()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        user.GenerateRefreshToken("token1", 7);
        user.GenerateRefreshToken("token2", 7);

        // Act
        user.Deactivate();

        // Assert
        user.RefreshTokens.Should().AllSatisfy(rt => rt.IsRevoked.Should().BeTrue());
    }

    [Fact]
    public void RecordLogin_ShouldUpdateLastLoginAt()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();

        // Act
        user.RecordLogin();

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RecordLogin_CalledMultipleTimes_ShouldUpdateToLatestTime()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();

        // Act
        user.RecordLogin();
        var firstLogin = user.LastLoginAt;
        Thread.Sleep(50);
        user.RecordLogin();
        var secondLogin = user.LastLoginAt;

        // Assert
        secondLogin.Should().BeAfter(firstLogin!.Value);
    }

    [Fact]
    public void AssignRole_ShouldAddRoleToUser()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var role = UserTestFactory.CreateRole("USER");
        typeof(Role).GetProperty("Id")!.SetValue(role, 10);

        // Act
        user.AssignRole(role);

        // Assert
        user.UserRoles.Should().ContainSingle()
            .Which.RoleId.Should().Be(10);
    }

    [Fact]
    public void AssignRole_ShouldRaiseRoleAssignedDomainEvent()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var role = UserTestFactory.CreateRole("ADMIN");
        typeof(Role).GetProperty("Id")!.SetValue(role, 10);

        // Act
        user.AssignRole(role);

        // Assert
        user.Notifications.Should().ContainSingle()
            .Which.Should().BeOfType<RoleAssignedDomainEvent>()
            .Which.RoleName.Should().Be("ADMIN");
    }

    [Fact]
    public void AssignRole_WhenRoleAlreadyAssigned_ShouldNotAddDuplicate()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var role = UserTestFactory.CreateRole("USER");
        typeof(Role).GetProperty("Id")!.SetValue(role, 10);

        // Act
        user.AssignRole(role);
        user.ClearNotification();
        user.AssignRole(role);

        // Assert
        user.UserRoles.Should().HaveCount(1);
        user.Notifications.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRole_ShouldRemoveRoleFromUser()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var role = UserTestFactory.CreateRole("USER");
        typeof(Role).GetProperty("Id")!.SetValue(role, 10);
        user.AssignRole(role);

        // Act
        user.RemoveRole(role);

        // Assert
        user.UserRoles.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRole_WhenRoleNotAssigned_ShouldDoNothing()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var role = UserTestFactory.CreateRole("USER");
        typeof(Role).GetProperty("Id")!.SetValue(role, 10);

        // Act
        user.RemoveRole(role);

        // Assert
        user.UserRoles.Should().BeEmpty();
    }

    [Fact]
    public void HasRole_WhenRoleAssigned_ShouldReturnTrue()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var role = UserTestFactory.CreateRole("USER");
        typeof(Role).GetProperty("Id")!.SetValue(role, 10);
        user.AssignRole(role);

        // Simulate navigation property being loaded
        var userRole = user.UserRoles.First();
        typeof(UserRole).GetProperty("Role")!.SetValue(userRole, role);

        // Act
        var hasRole = user.HasRole("USER");

        // Assert
        hasRole.Should().BeTrue();
    }

    [Fact]
    public void HasRole_WhenRoleNotAssigned_ShouldReturnFalse()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();

        // Act
        var hasRole = user.HasRole("ADMIN");

        // Assert
        hasRole.Should().BeFalse();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldCreateAndAddRefreshToken()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var token = "refresh-token-123";

        // Act
        var refreshToken = user.GenerateRefreshToken(token, 7);

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.Token.Should().Be(token);
        user.RefreshTokens.Should().Contain(refreshToken);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldSetCorrectExpiration()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var token = "refresh-token-123";
        var expirationDays = 14;

        // Act
        var refreshToken = user.GenerateRefreshToken(token, expirationDays);

        // Assert
        refreshToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(expirationDays), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RevokeRefreshToken_ShouldRevokeSpecificToken()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var token = "refresh-token-123";
        user.GenerateRefreshToken(token, 7);

        // Act
        user.RevokeRefreshToken(token);

        // Assert
        var refreshToken = user.RefreshTokens.First();
        refreshToken.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void RevokeRefreshToken_WhenTokenNotFound_ShouldDoNothing()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();

        // Act
        var act = () => user.RevokeRefreshToken("non-existent-token");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RevokeAllRefreshTokens_ShouldRevokeAllActiveTokens()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        user.GenerateRefreshToken("token1", 7);
        user.GenerateRefreshToken("token2", 7);
        user.GenerateRefreshToken("token3", 7);

        // Act
        user.RevokeAllRefreshTokens();

        // Assert
        user.RefreshTokens.Should().AllSatisfy(rt => rt.IsRevoked.Should().BeTrue());
    }

    [Fact]
    public void RevokeAllRefreshTokens_ShouldOnlyRevokeActiveTokens()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        user.GenerateRefreshToken("token1", 7);
        user.GenerateRefreshToken("token2", 7);
        user.RevokeRefreshToken("token1");

        // Act
        user.RevokeAllRefreshTokens();

        // Assert
        user.RefreshTokens.Should().AllSatisfy(rt => rt.IsRevoked.Should().BeTrue());
    }

    [Fact]
    public void GetActiveRefreshToken_WhenTokenIsActive_ShouldReturnToken()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var token = "refresh-token-123";
        user.GenerateRefreshToken(token, 7);

        // Act
        var refreshToken = user.GetActiveRefreshToken(token);

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken!.Token.Should().Be(token);
    }

    [Fact]
    public void GetActiveRefreshToken_WhenTokenIsRevoked_ShouldReturnNull()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var token = "refresh-token-123";
        user.GenerateRefreshToken(token, 7);
        user.RevokeRefreshToken(token);

        // Act
        var refreshToken = user.GetActiveRefreshToken(token);

        // Assert
        refreshToken.Should().BeNull();
    }

    [Fact]
    public void GetActiveRefreshToken_WhenTokenNotFound_ShouldReturnNull()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();

        // Act
        var refreshToken = user.GetActiveRefreshToken("non-existent-token");

        // Assert
        refreshToken.Should().BeNull();
    }

    [Fact]
    public void ValidatePassword_WithMatchingPassword_ShouldReturnTrue()
    {
        // Arrange
        var passwordHash = "ABCDEF0123456789";
        var user = UserTestFactory.CreateValidUser(passwordHash: passwordHash);
        var inputPassword = Password.FromHash(passwordHash);

        // Act
        var isValid = user.ValidatePassword(inputPassword);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void ValidatePassword_WithDifferentPassword_ShouldReturnFalse()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser(passwordHash: "ABCDEF0123456789");
        var inputPassword = Password.FromHash("DifferentHash");

        // Act
        var isValid = user.ValidatePassword(inputPassword);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void User_ShouldInitializeEmptyCollections()
    {
        // Arrange & Act
        var user = UserTestFactory.CreateValidUser();

        // Assert
        user.UserRoles.Should().NotBeNull().And.BeEmpty();
        user.RefreshTokens.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void User_Collections_ShouldBeReadOnly()
    {
        // Arrange & Act
        var user = UserTestFactory.CreateValidUser();

        // Assert
        user.UserRoles.Should().BeAssignableTo<IReadOnlyCollection<UserRole>>();
        user.RefreshTokens.Should().BeAssignableTo<IReadOnlyCollection<RefreshToken>>();
    }
}
