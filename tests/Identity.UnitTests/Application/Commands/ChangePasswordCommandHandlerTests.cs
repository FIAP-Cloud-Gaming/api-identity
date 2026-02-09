using Identity.API.Application.Commands.ChangePassword;
using Identity.UnitTests.Helpers;

namespace Identity.UnitTests.Application.Commands;

public sealed class ChangePasswordCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ChangePasswordCommandHandler _handler;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _userRepository.UnitOfWork.Returns(_unitOfWork);
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);

        _handler = new ChangePasswordCommandHandler(_userRepository, _passwordHasher);
    }

    [Fact]
    public async Task Handle_ValidCommand_ChangesPasswordSuccessfully()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser(
            "user@example.com",
            "John",
            "Doe",
            "old_hashed_password");

        var command = new ChangePasswordCommand(user.Id, "OldP@ssw0rd", "NewP@ssw0rd123");

        _userRepository.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify(command.CurrentPassword, user.PasswordHash.Hash).Returns(true);
        _passwordHasher.Hash(command.NewPassword).Returns("new_hashed_password");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.PasswordHash.Hash.Should().Be("new_hashed_password");
        _userRepository.Received(1).Update(user);
        await _unitOfWork.Received(1).SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsIdentityDomainException()
    {
        // Arrange
        var command = new ChangePasswordCommand(999, "OldP@ssw0rd", "NewP@ssw0rd123");

        _userRepository.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<IdentityDomainException>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task Handle_CurrentPasswordWrong_ThrowsIdentityDomainException()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser(
            "user@example.com",
            "John",
            "Doe",
            "old_hashed_password");

        var command = new ChangePasswordCommand(user.Id, "WrongPassword", "NewP@ssw0rd123");

        _userRepository.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify(command.CurrentPassword, user.PasswordHash.Hash).Returns(false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<IdentityDomainException>()
            .WithMessage("Current password is incorrect");
    }

    [Fact]
    public async Task Handle_NewPasswordWeak_ThrowsIdentityDomainException()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser(
            "user@example.com",
            "John",
            "Doe",
            "old_hashed_password");

        var command = new ChangePasswordCommand(user.Id, "OldP@ssw0rd", "weak");

        _userRepository.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify(command.CurrentPassword, user.PasswordHash.Hash).Returns(true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<IdentityDomainException>();
    }

    [Fact]
    public async Task Handle_ValidCommand_RevokesAllRefreshTokens()
    {
        // Arrange
        var user = UserTestFactory.CreateValidUser(
            "user@example.com",
            "John",
            "Doe",
            "old_hashed_password");

        user.GenerateRefreshToken("token1", 7);
        user.GenerateRefreshToken("token2", 7);

        var command = new ChangePasswordCommand(user.Id, "OldP@ssw0rd", "NewP@ssw0rd123");

        _userRepository.GetByIdAsync(command.UserId, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify(command.CurrentPassword, user.PasswordHash.Hash).Returns(true);
        _passwordHasher.Hash(command.NewPassword).Returns("new_hashed_password");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        user.RefreshTokens.Should().AllSatisfy(token =>
            token.IsRevoked.Should().BeTrue());
    }
}
