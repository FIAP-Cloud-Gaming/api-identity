using Identity.API.Application.Commands.ChangePassword;
using Identity.API.Application.Validations;

namespace Identity.UnitTests.Application.Validations;

public sealed class ChangePasswordCommandValidatorTests
{
    private readonly ChangePasswordCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new ChangePasswordCommand(1, "OldP@ssw0rd", "NewP@ssw0rd123");

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyCurrentPassword_FailsValidation(string? currentPassword)
    {
        // Arrange
        var command = new ChangePasswordCommand(1, currentPassword!, "NewP@ssw0rd123");

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CurrentPassword" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyNewPassword_FailsValidation(string? newPassword)
    {
        // Arrange
        var command = new ChangePasswordCommand(1, "OldP@ssw0rd", newPassword!);

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    public async Task Validate_NewPasswordTooShort_FailsValidation(string newPassword)
    {
        // Arrange
        var command = new ChangePasswordCommand(1, "OldP@ssw0rd", newPassword);

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("at least 8 characters"));
    }

    [Theory]
    [InlineData("alllowercase123")]
    [InlineData("nouppercase1")]
    public async Task Validate_NewPasswordNoUppercase_FailsValidation(string newPassword)
    {
        // Arrange
        var command = new ChangePasswordCommand(1, "OldP@ssw0rd", newPassword);

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("uppercase"));
    }

    [Theory]
    [InlineData("ALLUPPERCASE123")]
    [InlineData("NOLOWERCASE1")]
    public async Task Validate_NewPasswordNoLowercase_FailsValidation(string newPassword)
    {
        // Arrange
        var command = new ChangePasswordCommand(1, "OldP@ssw0rd", newPassword);

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("lowercase"));
    }

    [Theory]
    [InlineData("NoDigitsHere")]
    [InlineData("OnlyLetters")]
    public async Task Validate_NewPasswordNoDigit_FailsValidation(string newPassword)
    {
        // Arrange
        var command = new ChangePasswordCommand(1, "OldP@ssw0rd", newPassword);

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("number"));
    }

    [Fact]
    public async Task Validate_BothPasswordsEmpty_ReturnsMultipleErrors()
    {
        // Arrange
        var command = new ChangePasswordCommand(1, "", "");

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
        result.Errors.Should().Contain(e => e.PropertyName == "CurrentPassword");
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Fact]
    public async Task Validate_NewPasswordMultipleViolations_ReturnsAllErrors()
    {
        // Arrange
        var command = new ChangePasswordCommand(1, "OldP@ssw0rd", "weak");

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThan(1);
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Fact]
    public async Task Validate_CurrentPasswordWeakButValid_PassesValidation()
    {
        // Arrange - Current password is not validated for strength, only required
        var command = new ChangePasswordCommand(1, "weak", "NewP@ssw0rd123");

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("P@ssw0rd1")]
    [InlineData("Str0ngP@ss")]
    [InlineData("MyNewP@ss123")]
    public async Task Validate_ValidNewPassword_PassesValidation(string newPassword)
    {
        // Arrange
        var command = new ChangePasswordCommand(1, "OldP@ssw0rd", newPassword);

        // Act
        var result = await _validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
