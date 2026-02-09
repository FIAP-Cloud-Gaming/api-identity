using Identity.API.Application.Commands.RegisterUser;
using Identity.UnitTests.Helpers;

namespace Identity.UnitTests.Application.Commands;

public sealed class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<RegisterUserCommandHandler> _logger;
    private readonly RegisterUserCommandHandler _handler;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _logger = Substitute.For<ILogger<RegisterUserCommandHandler>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _userRepository.UnitOfWork.Returns(_unitOfWork);
        _unitOfWork.SaveEntitiesAsync(Arg.Any<CancellationToken>()).Returns(true);

        _handler = new RegisterUserCommandHandler(_userRepository, _passwordHasher, _logger);
    }

    [Fact]
    public async Task Handle_ValidCommand_RegistersUserSuccessfully()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "user@example.com",
            "StrongP@ss123",
            "John",
            "Doe");

        var userRole = UserTestFactory.CreateRole("USER", "Default user role", true);

        _userRepository.ExistsByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash(command.Password).Returns("hashed_password");
        _userRepository.GetRoleByNameAsync("USER", Arg.Any<CancellationToken>())
            .Returns(userRole);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(command.Email);
        result.FullName.Should().Be("John Doe");
        _userRepository.Received(1).Add(Arg.Is<User>(u =>
            u.Email.Value == command.Email &&
            u.Name.FirstName == command.FirstName &&
            u.Name.LastName == command.LastName));
        await _unitOfWork.Received(1).SaveEntitiesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailAlreadyExists_ThrowsIdentityDomainException()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "existing@example.com",
            "StrongP@ss123",
            "John",
            "Doe");

        _userRepository.ExistsByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<IdentityDomainException>()
            .WithMessage("Email already registered");
    }

    [Fact]
    public async Task Handle_WeakPassword_ThrowsIdentityDomainException()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "user@example.com",
            "weak",
            "John",
            "Doe");

        _userRepository.ExistsByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<IdentityDomainException>();
    }

    [Fact]
    public async Task Handle_ValidCommand_AssignsUserRole()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "user@example.com",
            "StrongP@ss123",
            "John",
            "Doe");

        var userRole = UserTestFactory.CreateRole("USER", "Default user role", true);

        _userRepository.ExistsByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash(command.Password).Returns("hashed_password");
        _userRepository.GetRoleByNameAsync("USER", Arg.Any<CancellationToken>())
            .Returns(userRole);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepository.Received(1).Add(Arg.Is<User>(u =>
            u.UserRoles.Count() == 1));
    }

    [Fact]
    public async Task Handle_NoUserRole_RegistersUserWithoutRole()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "user@example.com",
            "StrongP@ss123",
            "John",
            "Doe");

        _userRepository.ExistsByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash(command.Password).Returns("hashed_password");
        _userRepository.GetRoleByNameAsync("USER", Arg.Any<CancellationToken>())
            .Returns((Role?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _userRepository.Received(1).Add(Arg.Is<User>(u => !u.UserRoles.Any()));
    }
}
