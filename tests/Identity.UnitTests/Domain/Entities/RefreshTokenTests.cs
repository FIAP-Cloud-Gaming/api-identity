using Identity.UnitTests.Helpers;

namespace Identity.UnitTests.Domain.Entities;

public sealed class RefreshTokenTests
{
    [Fact]
    public void GenerateRefreshToken_ShouldCreateRefreshTokenWithCorrectProperties()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var token = "refresh-token-123";
        var expirationDays = 7;

        // Act
        var refreshToken = user.GenerateRefreshToken(token, expirationDays);

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.UserId.Should().Be(user.Id);
        refreshToken.Token.Should().Be(token);
        refreshToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(expirationDays), TimeSpan.FromSeconds(1));
        refreshToken.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        refreshToken.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void GenerateRefreshToken_WithDefaultExpiration_ShouldUse7Days()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var token = "refresh-token-123";

        // Act
        var refreshToken = user.GenerateRefreshToken(token);

        // Assert
        refreshToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RefreshToken_IsExpired_WhenExpired_ShouldReturnTrue()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var token = "refresh-token-123";
        var refreshToken = user.GenerateRefreshToken(token, expirationDays: 0);

        // Act
        Thread.Sleep(100);
        var isExpired = refreshToken.IsExpired;

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void RefreshToken_IsExpired_WhenNotExpired_ShouldReturnFalse()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var token = "refresh-token-123";
        var refreshToken = user.GenerateRefreshToken(token, expirationDays: 7);

        // Act
        var isExpired = refreshToken.IsExpired;

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_IsRevoked_WhenNotRevoked_ShouldReturnFalse()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var token = "refresh-token-123";
        var refreshToken = user.GenerateRefreshToken(token);

        // Act
        var isRevoked = refreshToken.IsRevoked;

        // Assert
        isRevoked.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_IsRevoked_WhenRevoked_ShouldReturnTrue()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var token = "refresh-token-123";
        var refreshToken = user.GenerateRefreshToken(token);

        // Act
        refreshToken.Revoke();
        var isRevoked = refreshToken.IsRevoked;

        // Assert
        isRevoked.Should().BeTrue();
        refreshToken.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RefreshToken_IsActive_WhenActiveAndNotExpired_ShouldReturnTrue()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var token = "refresh-token-123";
        var refreshToken = user.GenerateRefreshToken(token, expirationDays: 7);

        // Act
        var isActive = refreshToken.IsActive;

        // Assert
        isActive.Should().BeTrue();
    }

    [Fact]
    public void RefreshToken_IsActive_WhenRevoked_ShouldReturnFalse()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var token = "refresh-token-123";
        var refreshToken = user.GenerateRefreshToken(token, expirationDays: 7);

        // Act
        refreshToken.Revoke();
        var isActive = refreshToken.IsActive;

        // Assert
        isActive.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_IsActive_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var token = "refresh-token-123";
        var refreshToken = user.GenerateRefreshToken(token, expirationDays: 0);

        // Act
        Thread.Sleep(100);
        var isActive = refreshToken.IsActive;

        // Assert
        isActive.Should().BeFalse();
    }

    [Fact]
    public void Revoke_ShouldSetRevokedAt()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var token = "refresh-token-123";
        var refreshToken = user.GenerateRefreshToken(token);

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.RevokedAt.Should().NotBeNull();
        refreshToken.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Revoke_WhenCalledMultipleTimes_ShouldBeIdempotent()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var token = "refresh-token-123";
        var refreshToken = user.GenerateRefreshToken(token);

        // Act
        refreshToken.Revoke();
        var firstRevokedAt = refreshToken.RevokedAt;
        Thread.Sleep(50);
        refreshToken.Revoke();
        var secondRevokedAt = refreshToken.RevokedAt;

        // Assert
        firstRevokedAt.Should().Be(secondRevokedAt);
    }

    [Fact]
    public void RefreshToken_ShouldAddToUserRefreshTokensCollection()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser();
        var token = "refresh-token-123";

        // Act
        var refreshToken = user.GenerateRefreshToken(token);

        // Assert
        user.RefreshTokens.Should().Contain(refreshToken);
        user.RefreshTokens.Should().HaveCount(1);
    }
}
