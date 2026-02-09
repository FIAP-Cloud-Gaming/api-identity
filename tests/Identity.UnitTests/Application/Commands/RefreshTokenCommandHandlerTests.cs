using Identity.API.Application.Commands.RefreshToken;
using Identity.UnitTests.Helpers;

namespace Identity.UnitTests.Application.Commands;

public sealed class RefreshTokenCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly RefreshTokenCommandHandler _handler;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _tokenProvider = Substitute.For<ITokenProvider>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _userRepository.UnitOfWork.Returns(_unitOfWork);
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);

        _handler = new RefreshTokenCommandHandler(_userRepository, _tokenProvider);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_RotatesTokensSuccessfully()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser(
            "user@example.com",
            "John",
            "Doe",
            "hashed_password");

        var oldRefreshToken = "old_refresh_token";
        user.GenerateRefreshToken(oldRefreshToken, 7);

        var userRole = UserTestFactory.CreateRole("USER", "Default role", false);
        UserTestFactory.AssignRoleWithNavigation(user, userRole);

        var command = new RefreshTokenCommand(oldRefreshToken);

        var newAccessToken = new Token("new_access_token", DateTime.UtcNow.AddHours(1));
        var newRefreshToken = "new_refresh_token";

        _userRepository.GetByRefreshTokenAsync(oldRefreshToken, Arg.Any<CancellationToken>())
            .Returns(user);
        _tokenProvider.GenerateAccessToken(user, Arg.Any<List<string>>()).Returns(newAccessToken);
        _tokenProvider.GenerateRefreshToken().Returns(newRefreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be(user.Email.Value);
        result.AccessToken.Should().Be("new_access_token");
        result.RefreshToken.Should().Be(newRefreshToken);
        result.AccessTokenExpiration.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromSeconds(5));
        result.Roles.Should().ContainSingle().Which.Should().Be("USER");

        _userRepository.Received(1).Update(user);
        await _unitOfWork.Received(1).SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvalidRefreshToken_ThrowsIdentityDomainException()
    {
        // Arrange
        var command = new RefreshTokenCommand("invalid_token");

        _userRepository.GetByRefreshTokenAsync(command.RefreshToken, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<IdentityDomainException>()
            .WithMessage("Invalid refresh token");
    }

    [Fact]
    public async Task Handle_ValidToken_RevokesOldTokenAndGeneratesNew()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser(
            "user@example.com",
            "John",
            "Doe",
            "hashed_password");

        var oldRefreshToken = "old_refresh_token";
        user.GenerateRefreshToken(oldRefreshToken, 7);

        var command = new RefreshTokenCommand(oldRefreshToken);

        var newAccessToken = new Token("new_access_token", DateTime.UtcNow.AddHours(1));
        var newRefreshToken = "new_refresh_token";

        _userRepository.GetByRefreshTokenAsync(oldRefreshToken, Arg.Any<CancellationToken>())
            .Returns(user);
        _tokenProvider.GenerateAccessToken(user, Arg.Any<List<string>>()).Returns(newAccessToken);
        _tokenProvider.GenerateRefreshToken().Returns(newRefreshToken);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var revokedToken = user.RefreshTokens.FirstOrDefault(t => t.Token == oldRefreshToken);
        revokedToken.Should().NotBeNull();
        revokedToken!.IsRevoked.Should().BeTrue();

        var newToken = user.RefreshTokens.FirstOrDefault(t => t.Token == newRefreshToken);
        newToken.Should().NotBeNull();
        newToken!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidToken_ReturnsUserWithAllRoles()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser(
            "user@example.com",
            "John",
            "Doe",
            "hashed_password");

        var oldRefreshToken = "old_refresh_token";
        user.GenerateRefreshToken(oldRefreshToken, 7);

        var userRole = UserTestFactory.CreateRole("USER", "User role", false);
        var adminRole = UserTestFactory.CreateRole("ADMIN", "Admin role", false);
        UserTestFactory.AssignRoleWithNavigation(user, userRole);
        UserTestFactory.AssignRoleWithNavigation(user, adminRole);

        var command = new RefreshTokenCommand(oldRefreshToken);

        var newAccessToken = new Token("new_access_token", DateTime.UtcNow.AddHours(1));
        var newRefreshToken = "new_refresh_token";

        _userRepository.GetByRefreshTokenAsync(oldRefreshToken, Arg.Any<CancellationToken>())
            .Returns(user);
        _tokenProvider.GenerateAccessToken(user, Arg.Any<List<string>>()).Returns(newAccessToken);
        _tokenProvider.GenerateRefreshToken().Returns(newRefreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Roles.Should().HaveCount(2);
        result.Roles.Should().Contain("USER");
        result.Roles.Should().Contain("ADMIN");
    }
}
