using Identity.API.Application.Commands.Login;
using Identity.API.Application.Validations;

namespace Identity.UnitTests.Application.Validations;

public sealed class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "P@ssw0rd123");

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyEmail_FailsValidation(string? email)
    {
        // Arrange
        var command = new LoginCommand(email!, "P@ssw0rd123");

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user")]
    [InlineData("user.example.com")]
    public async Task Validate_InvalidEmailFormat_FailsValidation(string email)
    {
        // Arrange
        var command = new LoginCommand(email, "P@ssw0rd123");

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("Invalid email format"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyPassword_FailsValidation(string? password)
    {
        // Arrange
        var command = new LoginCommand("user@example.com", password!);

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public async Task Validate_BothFieldsEmpty_ReturnsMultipleErrors()
    {
        // Arrange
        var command = new LoginCommand("", "");

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        // Email empty triggers both NotEmpty and EmailAddress = 2 errors, Password = 1 error = 3 total
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public async Task Validate_ValidEmailAndPassword_AcceptsAnyPasswordFormat()
    {
        // Arrange - Note: Login validator doesn't validate password strength
        var command = new LoginCommand("user@example.com", "weak");

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("user@example.com", "short")]
    [InlineData("test@domain.com", "1")]
    [InlineData("admin@test.org", "password")]
    public async Task Validate_ValidEmailWithAnyPassword_PassesValidation(string email, string password)
    {
        // Arrange
        var command = new LoginCommand(email, password);

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
