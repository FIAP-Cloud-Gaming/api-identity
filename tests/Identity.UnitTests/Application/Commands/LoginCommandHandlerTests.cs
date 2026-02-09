using Identity.API.Application.Commands.Login;
using Identity.UnitTests.Helpers;

namespace Identity.UnitTests.Application.Commands;

public sealed class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenProvider _tokenProvider;
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly LoginCommandHandler _handler;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tokenProvider = Substitute.For<ITokenProvider>();
        _logger = Substitute.For<ILogger<LoginCommandHandler>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _userRepository.UnitOfWork.Returns(_unitOfWork);
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);

        _handler = new LoginCommandHandler(_userRepository, _passwordHasher, _tokenProvider, _logger);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsLoginResponseWithTokens()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "P@ssw0rd123");
        var user = UserTestFactory.CreateValidUser(
            "user@example.com",
            "John",
            "Doe",
            "hashed_password");

        var userRole = UserTestFactory.CreateRole("USER", "Default role", false);
        UserTestFactory.AssignRoleWithNavigation(user, userRole);

        var accessToken = new Token("access_token_value", DateTime.UtcNow.AddHours(1));
        var refreshToken = "refresh_token_value";

        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify(command.Password, user.PasswordHash.Hash).Returns(true);
        _tokenProvider.GenerateAccessToken(user, Arg.Any<List<string>>()).Returns(accessToken);
        _tokenProvider.GenerateRefreshToken().Returns(refreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be(user.Email.Value);
        result.FullName.Should().Be("John Doe");
        result.AccessToken.Should().Be("access_token_value");
        result.RefreshToken.Should().Be(refreshToken);
        result.AccessTokenExpiration.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromSeconds(5));
        result.Roles.Should().ContainSingle().Which.Should().Be("USER");

        _userRepository.Received(1).Update(user);
        await _unitOfWork.Received(1).SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsIdentityDomainException()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@example.com", "P@ssw0rd123");

        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<IdentityDomainException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task Handle_UserNotActive_ThrowsIdentityDomainException()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "P@ssw0rd123");
        var user = UserTestFactory.CreateValidUser(
            "user@example.com",
            "John",
            "Doe",
            "hashed_password");
        user.Deactivate();

        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(user);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<IdentityDomainException>()
            .WithMessage("User account is not active");
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsIdentityDomainException()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "WrongPassword");
        var user = UserTestFactory.CreateValidUser(
            "user@example.com",
            "John",
            "Doe",
            "hashed_password");

        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify(command.Password, user.PasswordHash.Hash).Returns(false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<IdentityDomainException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task Handle_ValidLogin_RecordsLoginAndGeneratesRefreshToken()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "P@ssw0rd123");
        var user = UserTestFactory.CreateValidUser(
            "user@example.com",
            "John",
            "Doe",
            "hashed_password");

        var accessToken = new Token("access_token_value", DateTime.UtcNow.AddHours(1));
        var refreshToken = "refresh_token_value";

        _userRepository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify(command.Password, user.PasswordHash.Hash).Returns(true);
        _tokenProvider.GenerateAccessToken(user, Arg.Any<List<string>>()).Returns(accessToken);
        _tokenProvider.GenerateRefreshToken().Returns(refreshToken);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.RefreshTokens.Should().ContainSingle()
            .Which.Token.Should().Be(refreshToken);
    }
}
