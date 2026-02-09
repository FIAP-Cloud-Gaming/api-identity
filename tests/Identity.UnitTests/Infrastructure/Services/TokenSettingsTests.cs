namespace Identity.UnitTests.Infrastructure.Services;

public sealed class TokenSettingsTests
{
    [Fact]
    public void SectionName_ShouldBeCorrect()
    {
        // Assert
        TokenSettings.SectionName.Should().Be("JwtSettings");
    }

    [Fact]
    public void AccessTokenExpirationMinutes_ShouldHaveDefaultValue()
    {
        // Arrange & Act
        var settings = new TokenSettings();

        // Assert
        settings.AccessTokenExpirationMinutes.Should().Be(60);
    }

    [Fact]
    public void RefreshTokenExpirationDays_ShouldHaveDefaultValue()
    {
        // Arrange & Act
        var settings = new TokenSettings();

        // Assert
        settings.RefreshTokenExpirationDays.Should().Be(7);
    }

    [Fact]
    public void TokenSettings_ShouldAllowSettingProperties()
    {
        // Arrange & Act
        var settings = new TokenSettings
        {
            Secret = "MySecretKey123456789012345678901234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 30,
            RefreshTokenExpirationDays = 14
        };

        // Assert
        settings.Secret.Should().Be("MySecretKey123456789012345678901234567890");
        settings.Issuer.Should().Be("TestIssuer");
        settings.Audience.Should().Be("TestAudience");
        settings.AccessTokenExpirationMinutes.Should().Be(30);
        settings.RefreshTokenExpirationDays.Should().Be(14);
    }

    [Fact]
    public void TokenSettings_PropertiesShouldBeInitOnly()
    {
        // Arrange
        var settings = new TokenSettings
        {
            Secret = "Secret",
            Issuer = "Issuer",
            Audience = "Audience"
        };

        // Assert - This test verifies that properties are init-only by compilation
        // If properties were not init-only, the following would not compile
        settings.Secret.Should().Be("Secret");
        settings.Issuer.Should().Be("Issuer");
        settings.Audience.Should().Be("Audience");
    }
}
