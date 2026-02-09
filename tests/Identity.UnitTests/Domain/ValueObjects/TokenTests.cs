namespace Identity.UnitTests.Domain.ValueObjects;

public sealed class TokenTests
{
    [Fact]
    public void Token_WithValidValueAndFutureExpiration_ShouldCreateSuccessfully()
    {
        // Arrange
        var value = "test-token-123";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act
        var token = new Token(value, expiresAt);

        // Assert
        token.Value.Should().Be(value);
        token.ExpiresAt.Should().Be(expiresAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Token_WithNullOrWhitespaceValue_ShouldThrowIdentityDomainException(string? invalidValue)
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddHours(1);

        // Act
        var act = () => new Token(invalidValue!, expiresAt);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("Token value is required");
    }

    [Fact]
    public void Token_WithPastExpiration_ShouldThrowIdentityDomainException()
    {
        // Arrange
        var value = "test-token";
        var pastDate = DateTime.UtcNow.AddHours(-1);

        // Act
        var act = () => new Token(value, pastDate);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("Token expiration must be in the future");
    }

    [Fact]
    public void Token_WithCurrentTime_ShouldThrowIdentityDomainException()
    {
        // Arrange
        var value = "test-token";
        var now = DateTime.UtcNow;

        // Act
        var act = () => new Token(value, now);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("Token expiration must be in the future");
    }

    [Fact]
    public void Token_IsExpired_WhenExpired_ShouldReturnTrue()
    {
        // Arrange
        var value = "test-token";
        var expiresAt = DateTime.UtcNow.AddMilliseconds(100);
        var token = new Token(value, expiresAt);

        // Act
        Thread.Sleep(150);
        var isExpired = token.IsExpired;

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void Token_IsExpired_WhenNotExpired_ShouldReturnFalse()
    {
        // Arrange
        var value = "test-token";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var token = new Token(value, expiresAt);

        // Act
        var isExpired = token.IsExpired;

        // Assert
        isExpired.Should().BeFalse();
    }

    [Fact]
    public void Token_IsExpired_AtExactExpirationTime_ShouldReturnTrue()
    {
        // Arrange
        var value = "test-token";
        var expiresAt = DateTime.UtcNow.AddSeconds(1);
        var token = new Token(value, expiresAt);

        // Act
        Thread.Sleep(1100);
        var isExpired = token.IsExpired;

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void Token_ShouldSupportImplicitConversionToString()
    {
        // Arrange
        var value = "test-token-123";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var token = new Token(value, expiresAt);

        // Act
        string tokenString = token;

        // Assert
        tokenString.Should().Be(value);
    }

    [Fact]
    public void Token_ShouldSupportEqualityComparison()
    {
        // Arrange
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var token1 = new Token("token-123", expiresAt);
        var token2 = new Token("token-123", expiresAt);
        var token3 = new Token("different-token", expiresAt);

        // Assert
        token1.Should().Be(token2);
        token1.Should().NotBe(token3);
        (token1 == token2).Should().BeTrue();
        (token1 == token3).Should().BeFalse();
    }

    [Fact]
    public void Token_WithDifferentExpirations_ShouldNotBeEqual()
    {
        // Arrange
        var expiresAt1 = DateTime.UtcNow.AddHours(1);
        var expiresAt2 = DateTime.UtcNow.AddHours(2);
        var token1 = new Token("token-123", expiresAt1);
        var token2 = new Token("token-123", expiresAt2);

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void Token_WithFutureExpiration_ShouldBeValid()
    {
        // Arrange
        var value = "test-token";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var token = new Token(value, expiresAt);

        // Assert
        token.IsExpired.Should().BeFalse();
        token.Value.Should().Be(value);
        token.ExpiresAt.Should().Be(expiresAt);
    }
}
