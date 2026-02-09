using Identity.API.Application.Commands.RegisterUser;
using Identity.API.Application.Validations;

namespace Identity.UnitTests.Application.Validations;

public sealed class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "user@example.com",
            "StrongP@ss123",
            "John",
            "Doe");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyEmail_FailsValidation(string email)
    {
        // Arrange
        var command = new RegisterUserCommand(email, "StrongP@ss123", "John", "Doe");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user")]
    public async Task Validate_InvalidEmailFormat_FailsValidation(string email)
    {
        // Arrange
        var command = new RegisterUserCommand(email, "StrongP@ss123", "John", "Doe");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("Invalid email format"));
    }

    [Fact]
    public async Task Validate_EmailTooLong_FailsValidation()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@example.com";
        var command = new RegisterUserCommand(longEmail, "StrongP@ss123", "John", "Doe");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyPassword_FailsValidation(string password)
    {
        // Arrange
        var command = new RegisterUserCommand("user@example.com", password, "John", "Doe");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public async Task Validate_PasswordTooShort_FailsValidation(string password)
    {
        // Arrange
        var command = new RegisterUserCommand("user@example.com", password, "John", "Doe");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("at least 8 characters"));
    }

    [Theory]
    [InlineData("alllowercase123")]
    [InlineData("nouppercase1")]
    public async Task Validate_PasswordNoUppercase_FailsValidation(string password)
    {
        // Arrange
        var command = new RegisterUserCommand("user@example.com", password, "John", "Doe");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("uppercase"));
    }

    [Theory]
    [InlineData("ALLUPPERCASE123")]
    [InlineData("NOLOWERCASE1")]
    public async Task Validate_PasswordNoLowercase_FailsValidation(string password)
    {
        // Arrange
        var command = new RegisterUserCommand("user@example.com", password, "John", "Doe");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("lowercase"));
    }

    [Theory]
    [InlineData("NoDigitsHere")]
    [InlineData("OnlyLetters")]
    public async Task Validate_PasswordNoDigit_FailsValidation(string password)
    {
        // Arrange
        var command = new RegisterUserCommand("user@example.com", password, "John", "Doe");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("number"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyFirstName_FailsValidation(string firstName)
    {
        // Arrange
        var command = new RegisterUserCommand("user@example.com", "StrongP@ss123", firstName, "Doe");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public async Task Validate_FirstNameTooLong_FailsValidation()
    {
        // Arrange
        var longFirstName = new string('a', 101);
        var command = new RegisterUserCommand("user@example.com", "StrongP@ss123", longFirstName, "Doe");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyLastName_FailsValidation(string lastName)
    {
        // Arrange
        var command = new RegisterUserCommand("user@example.com", "StrongP@ss123", "John", lastName);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LastName" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public async Task Validate_LastNameTooLong_FailsValidation()
    {
        // Arrange
        var longLastName = new string('a', 101);
        var command = new RegisterUserCommand("user@example.com", "StrongP@ss123", "John", longLastName);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LastName");
    }

    [Fact]
    public async Task Validate_MultipleErrors_ReturnsAllFailures()
    {
        // Arrange
        var command = new RegisterUserCommand("invalid-email", "weak", "", "");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(3);
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
        result.Errors.Should().Contain(e => e.PropertyName == "LastName");
    }
}
