using Identity.API.Application.Behaviors;
using Identity.API.Application.Commands.RegisterUser;
using Identity.API.Application.Commands.ChangePassword;
using FluentValidation;
using FluentValidation.Results;

namespace Identity.UnitTests.Application.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithResponse_NoValidators_PassesThrough()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<RegisterUserCommand>>();
        var behavior = new ValidationBehavior<RegisterUserCommand, RegisterUserResult>(validators);

        var command = new RegisterUserCommand("user@example.com", "P@ssw0rd123", "John", "Doe");
        var expectedResult = new RegisterUserResult(1, "user@example.com", "John Doe");

        var nextCalled = false;
        RequestHandlerDelegate<RegisterUserResult> next = (ct) =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithoutResponse_NoValidators_PassesThrough()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<ChangePasswordCommand>>();
        var behavior = new ValidationBehavior<ChangePasswordCommand>(validators);

        var command = new ChangePasswordCommand(1, "OldP@ss", "NewP@ssw0rd123");

        var nextCalled = false;
        RequestHandlerDelegate next = (ct) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        // Act
        await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithResponse_ValidRequest_PassesValidation()
    {
        // Arrange
        var validator = Substitute.For<IValidator<RegisterUserCommand>>();
        var validators = new[] { validator };
        var behavior = new ValidationBehavior<RegisterUserCommand, RegisterUserResult>(validators);

        var command = new RegisterUserCommand("user@example.com", "P@ssw0rd123", "John", "Doe");
        var expectedResult = new RegisterUserResult(1, "user@example.com", "John Doe");

        validator.ValidateAsync(Arg.Any<ValidationContext<RegisterUserCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var nextCalled = false;
        RequestHandlerDelegate<RegisterUserResult> next = (ct) =>
        {
            nextCalled = true;
            return Task.FromResult(expectedResult);
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithoutResponse_ValidRequest_PassesValidation()
    {
        // Arrange
        var validator = Substitute.For<IValidator<ChangePasswordCommand>>();
        var validators = new[] { validator };
        var behavior = new ValidationBehavior<ChangePasswordCommand>(validators);

        var command = new ChangePasswordCommand(1, "OldP@ss", "NewP@ssw0rd123");

        validator.ValidateAsync(Arg.Any<ValidationContext<ChangePasswordCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var nextCalled = false;
        RequestHandlerDelegate next = (ct) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        // Act
        await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithResponse_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var validator = Substitute.For<IValidator<RegisterUserCommand>>();
        var validators = new[] { validator };
        var behavior = new ValidationBehavior<RegisterUserCommand, RegisterUserResult>(validators);

        var command = new RegisterUserCommand("invalid-email", "weak", "", "");

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Email", "Invalid email format"),
            new ValidationFailure("Password", "Password is too weak"),
            new ValidationFailure("FirstName", "First name is required")
        };

        validator.ValidateAsync(Arg.Any<ValidationContext<RegisterUserCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(validationFailures));

        RequestHandlerDelegate<RegisterUserResult> next = (ct) =>
            Task.FromResult(new RegisterUserResult(1, "user@example.com", "John Doe"));

        // Act
        var act = async () => await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Count() == 3);
    }

    [Fact]
    public async Task Handle_WithoutResponse_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var validator = Substitute.For<IValidator<ChangePasswordCommand>>();
        var validators = new[] { validator };
        var behavior = new ValidationBehavior<ChangePasswordCommand>(validators);

        var command = new ChangePasswordCommand(1, "", "weak");

        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("CurrentPassword", "Current password is required"),
            new ValidationFailure("NewPassword", "Password must be at least 8 characters")
        };

        validator.ValidateAsync(Arg.Any<ValidationContext<ChangePasswordCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(validationFailures));

        RequestHandlerDelegate next = (ct) => Task.CompletedTask;

        // Act
        var act = async () => await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Count() == 2);
    }

    [Fact]
    public async Task Handle_WithResponse_MultipleValidators_CombinesAllFailures()
    {
        // Arrange
        var validator1 = Substitute.For<IValidator<RegisterUserCommand>>();
        var validator2 = Substitute.For<IValidator<RegisterUserCommand>>();
        var validators = new[] { validator1, validator2 };
        var behavior = new ValidationBehavior<RegisterUserCommand, RegisterUserResult>(validators);

        var command = new RegisterUserCommand("user@example.com", "P@ssw0rd123", "John", "Doe");

        validator1.ValidateAsync(Arg.Any<ValidationContext<RegisterUserCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Email", "Email already exists") }));

        validator2.ValidateAsync(Arg.Any<ValidationContext<RegisterUserCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { new ValidationFailure("Password", "Password is compromised") }));

        RequestHandlerDelegate<RegisterUserResult> next = (ct) =>
            Task.FromResult(new RegisterUserResult(1, "user@example.com", "John Doe"));

        // Act
        var act = async () => await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .Where(ex => ex.Errors.Count() == 2);
    }
}
