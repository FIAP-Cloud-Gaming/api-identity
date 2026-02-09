using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Identity.UnitTests.Helpers;

namespace Identity.UnitTests.Infrastructure.Services;

public sealed class TokenProviderTests
{
    private readonly TokenSettings _tokenSettings;
    private readonly ITokenProvider _tokenProvider;

    public TokenProviderTests()
    {
        _tokenSettings = new TokenSettings
        {
            Secret = "ThisIsAVerySecureSecretKeyForTesting1234567890",
            Issuer = "IdentityService",
            Audience = "IdentityServiceUsers",
            AccessTokenExpirationMinutes = 30,
            RefreshTokenExpirationDays = 7
        };

        var options = Options.Create(_tokenSettings);
        _tokenProvider = new TokenProvider(options);
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com", "John", "Doe");
        var roles = new[] { "USER", "ADMIN" };

        // Act
        var token = _tokenProvider.GenerateAccessToken(user, roles);

        // Assert
        token.Should().NotBeNull();
        token.Value.Should().NotBeNullOrEmpty();
        token.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainSubClaim()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com", "John", "Doe");
        var roles = new[] { "USER" };

        // Act
        var token = _tokenProvider.GenerateAccessToken(user, roles);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token.Value);

        // Assert
        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        subClaim.Should().NotBeNull();
        subClaim!.Value.Should().Be(user.Id.ToString());
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainEmailClaim()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com", "John", "Doe");
        var roles = new[] { "USER" };

        // Act
        var token = _tokenProvider.GenerateAccessToken(user, roles);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token.Value);

        // Assert
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        emailClaim.Should().NotBeNull();
        emailClaim!.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainNameClaim()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com", "John", "Doe");
        var roles = new[] { "USER" };

        // Act
        var token = _tokenProvider.GenerateAccessToken(user, roles);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token.Value);

        // Assert
        var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "name");
        nameClaim.Should().NotBeNull();
        nameClaim!.Value.Should().Be("John Doe");
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainRoleClaims()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com", "John", "Doe");
        var roles = new[] { "USER", "ADMIN" };

        // Act
        var token = _tokenProvider.GenerateAccessToken(user, roles);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token.Value);

        // Assert
        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        roleClaims.Should().HaveCount(2);
        roleClaims.Select(c => c.Value).Should().Contain(new[] { "USER", "ADMIN" });
    }

    [Fact]
    public void GenerateAccessToken_ShouldSetCorrectExpiration()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com", "John", "Doe");
        var roles = new[] { "USER" };
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _tokenProvider.GenerateAccessToken(user, roles);

        // Assert
        var expectedExpiration = beforeGeneration.AddMinutes(_tokenSettings.AccessTokenExpirationMinutes);
        token.ExpiresAt.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainJtiClaim()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com", "John", "Doe");
        var roles = new[] { "USER" };

        // Act
        var token = _tokenProvider.GenerateAccessToken(user, roles);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token.Value);

        // Assert
        var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
        jtiClaim.Should().NotBeNull();
        Guid.TryParse(jtiClaim!.Value, out _).Should().BeTrue("JTI should be a valid GUID");
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64String()
    {
        // Act
        var refreshToken = _tokenProvider.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
        var bytes = Convert.FromBase64String(refreshToken);
        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueTokens()
    {
        // Act
        var token1 = _tokenProvider.GenerateRefreshToken();
        var token2 = _tokenProvider.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2, "each refresh token should be unique");
    }

    [Fact]
    public void ValidateToken_ShouldReturnClaimsPrincipal_WhenTokenIsValid()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com", "John", "Doe");
        var roles = new[] { "USER" };
        var token = _tokenProvider.GenerateAccessToken(user, roles);

        // Act
        var principal = _tokenProvider.ValidateToken(token.Value);

        // Assert
        principal.Should().NotBeNull();
        principal!.Identity.Should().NotBeNull();
        principal.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_WhenTokenIsInvalid()
    {
        // Arrange
        const string invalidToken = "invalid.jwt.token";

        // Act
        var principal = _tokenProvider.ValidateToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_WhenTokenIsExpired()
    {
        // Arrange
        var expiredSettings = new TokenSettings
        {
            Secret = "ThisIsAVerySecureSecretKeyForTesting1234567890",
            Issuer = "IdentityService",
            Audience = "IdentityServiceUsers",
            AccessTokenExpirationMinutes = -1 // Expired token
        };
        var options = Options.Create(expiredSettings);
        var provider = new TokenProvider(options);
        var user = UserTestFactory.CreateValidUser("test@example.com", "John", "Doe");
        var roles = new[] { "USER" };

        // Act & Assert
        var act = () => provider.GenerateAccessToken(user, roles);
        act.Should().Throw<Exception>("token with past expiration should not be created");
    }

    [Fact]
    public void ValidateToken_ShouldReturnClaimsPrincipalWithCorrectClaims()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser("test@example.com", "John", "Doe");
        var roles = new[] { "USER", "ADMIN" };
        var token = _tokenProvider.GenerateAccessToken(user, roles);

        // Act
        var principal = _tokenProvider.ValidateToken(token.Value);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst(JwtRegisteredClaimNames.Sub)?.Value.Should().Be(user.Id.ToString());
        principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value.Should().Be("test@example.com");
        principal.FindAll(ClaimTypes.Role).Should().HaveCount(2);
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_WhenIssuerIsInvalid()
    {
        // Arrange
        var differentSettings = new TokenSettings
        {
            Secret = "ThisIsAVerySecureSecretKeyForTesting1234567890",
            Issuer = "DifferentIssuer",
            Audience = "IdentityServiceUsers",
            AccessTokenExpirationMinutes = 30
        };
        var options = Options.Create(differentSettings);
        var differentProvider = new TokenProvider(options);
        var user = UserTestFactory.CreateValidUser("test@example.com", "John", "Doe");
        var roles = new[] { "USER" };
        var token = differentProvider.GenerateAccessToken(user, roles);

        // Act
        var principal = _tokenProvider.ValidateToken(token.Value);

        // Assert
        principal.Should().BeNull("token with different issuer should not be valid");
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_WhenAudienceIsInvalid()
    {
        // Arrange
        var differentSettings = new TokenSettings
        {
            Secret = "ThisIsAVerySecureSecretKeyForTesting1234567890",
            Issuer = "IdentityService",
            Audience = "DifferentAudience",
            AccessTokenExpirationMinutes = 30
        };
        var options = Options.Create(differentSettings);
        var differentProvider = new TokenProvider(options);
        var user = UserTestFactory.CreateValidUser("test@example.com", "John", "Doe");
        var roles = new[] { "USER" };
        var token = differentProvider.GenerateAccessToken(user, roles);

        // Act
        var principal = _tokenProvider.ValidateToken(token.Value);

        // Assert
        principal.Should().BeNull("token with different audience should not be valid");
    }
}
