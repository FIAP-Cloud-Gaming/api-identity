using System.Security.Claims;

namespace Identity.UnitTests.Infrastructure.Services;

public sealed class IdentityServiceTests
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DefaultHttpContext _httpContext;

    public IdentityServiceTests()
    {
        _httpContext = new DefaultHttpContext();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContextAccessor.HttpContext.Returns(_httpContext);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenHttpContextAccessorIsNull()
    {
        // Act
        var act = () => new IdentityService(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("httpContextAccessor");
    }

    [Fact]
    public void GetUserIdentity_ShouldReturnNameIdentifierClaim_WhenPresent()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "12345")
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var service = new IdentityService(_httpContextAccessor);

        // Act
        var userId = service.GetUserIdentity();

        // Assert
        userId.Should().Be("12345");
    }

    [Fact]
    public void GetUserIdentity_ShouldReturnSubClaim_WhenNameIdentifierNotPresent()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("sub", "67890")
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var service = new IdentityService(_httpContextAccessor);

        // Act
        var userId = service.GetUserIdentity();

        // Assert
        userId.Should().Be("67890");
    }

    [Fact]
    public void GetUserIdentity_ShouldReturnNull_WhenNoClaimsPresent()
    {
        // Arrange
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        var service = new IdentityService(_httpContextAccessor);

        // Act
        var userId = service.GetUserIdentity();

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void GetUserIdentity_ShouldReturnNull_WhenHttpContextIsNull()
    {
        // Arrange
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);
        var service = new IdentityService(accessor);

        // Act
        var userId = service.GetUserIdentity();

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void GetUserName_ShouldReturnName_WhenClaimPresent()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "John Doe")
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var service = new IdentityService(_httpContextAccessor);

        // Act
        var userName = service.GetUserName();

        // Assert
        userName.Should().Be("John Doe");
    }

    [Fact]
    public void GetUserName_ShouldReturnNull_WhenClaimNotPresent()
    {
        // Arrange
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        var service = new IdentityService(_httpContextAccessor);

        // Act
        var userName = service.GetUserName();

        // Assert
        userName.Should().BeNull();
    }

    [Fact]
    public void GetUserEmail_ShouldReturnEmail_WhenClaimPresent()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var service = new IdentityService(_httpContextAccessor);

        // Act
        var email = service.GetUserEmail();

        // Assert
        email.Should().Be("test@example.com");
    }

    [Fact]
    public void GetUserEmail_ShouldReturnNull_WhenClaimNotPresent()
    {
        // Arrange
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        var service = new IdentityService(_httpContextAccessor);

        // Act
        var email = service.GetUserEmail();

        // Assert
        email.Should().BeNull();
    }

    [Fact]
    public void IsAuthenticated_ShouldReturnTrue_WhenUserIsAuthenticated()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "12345")
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var service = new IdentityService(_httpContextAccessor);

        // Act
        var isAuthenticated = service.IsAuthenticated();

        // Assert
        isAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_ShouldReturnFalse_WhenUserIsNotAuthenticated()
    {
        // Arrange
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        var service = new IdentityService(_httpContextAccessor);

        // Act
        var isAuthenticated = service.IsAuthenticated();

        // Assert
        isAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticated_ShouldReturnFalse_WhenHttpContextIsNull()
    {
        // Arrange
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);
        var service = new IdentityService(accessor);

        // Act
        var isAuthenticated = service.IsAuthenticated();

        // Assert
        isAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void IsInRole_ShouldReturnTrue_WhenUserHasRole()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "ADMIN")
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var service = new IdentityService(_httpContextAccessor);

        // Act
        var isInRole = service.IsInRole("ADMIN");

        // Assert
        isInRole.Should().BeTrue();
    }

    [Fact]
    public void IsInRole_ShouldReturnFalse_WhenUserDoesNotHaveRole()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "USER")
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var service = new IdentityService(_httpContextAccessor);

        // Act
        var isInRole = service.IsInRole("ADMIN");

        // Assert
        isInRole.Should().BeFalse();
    }

    [Fact]
    public void IsInRole_ShouldReturnFalse_WhenRoleIsNullOrEmpty()
    {
        // Arrange
        var service = new IdentityService(_httpContextAccessor);

        // Act
        var isInRoleNull = service.IsInRole(null!);
        var isInRoleEmpty = service.IsInRole(string.Empty);
        var isInRoleWhitespace = service.IsInRole("   ");

        // Assert
        isInRoleNull.Should().BeFalse();
        isInRoleEmpty.Should().BeFalse();
        isInRoleWhitespace.Should().BeFalse();
    }

    [Fact]
    public void IsInRole_ShouldReturnFalse_WhenHttpContextIsNull()
    {
        // Arrange
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);
        var service = new IdentityService(accessor);

        // Act
        var isInRole = service.IsInRole("ADMIN");

        // Assert
        isInRole.Should().BeFalse();
    }

    [Fact]
    public void GetUserRoles_ShouldReturnRoles_WhenUserHasRoles()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, "ADMIN"),
            new Claim(ClaimTypes.Role, "USER"),
            new Claim(ClaimTypes.Role, "MANAGER")
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var service = new IdentityService(_httpContextAccessor);

        // Act
        var roles = service.GetUserRoles();

        // Assert
        roles.Should().HaveCount(3);
        roles.Should().Contain(new[] { "ADMIN", "USER", "MANAGER" });
    }

    [Fact]
    public void GetUserRoles_ShouldReturnEmpty_WhenUserHasNoRoles()
    {
        // Arrange
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        var service = new IdentityService(_httpContextAccessor);

        // Act
        var roles = service.GetUserRoles();

        // Assert
        roles.Should().BeEmpty();
    }

    [Fact]
    public void GetUserRoles_ShouldReturnEmpty_WhenHttpContextIsNull()
    {
        // Arrange
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);
        var service = new IdentityService(accessor);

        // Act
        var roles = service.GetUserRoles();

        // Assert
        roles.Should().BeEmpty();
    }
}
