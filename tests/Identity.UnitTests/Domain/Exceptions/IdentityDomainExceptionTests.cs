namespace Identity.UnitTests.Domain.Exceptions;

public sealed class IdentityDomainExceptionTests
{
    [Fact]
    public void Constructor_WithNoParameters_ShouldCreateException()
    {
        // Act
        var exception = new IdentityDomainException();

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<IdentityDomainException>();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "This is a test error message";

        // Act
        var exception = new IdentityDomainException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        var message = "Outer exception message";
        var innerException = new InvalidOperationException("Inner exception message");

        // Act
        var exception = new IdentityDomainException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
        exception.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void IdentityDomainException_ShouldInheritFromException()
    {
        // Act
        var exception = new IdentityDomainException();

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void Constructor_WithNullMessage_ShouldNotThrow()
    {
        // Act
        var act = () => new IdentityDomainException(null!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithEmptyMessage_ShouldNotThrow()
    {
        // Act
        var act = () => new IdentityDomainException(string.Empty);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithMessageAndNullInnerException_ShouldNotThrow()
    {
        // Act
        var act = () => new IdentityDomainException("Test message", null!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void IdentityDomainException_CanBeCaught()
    {
        // Arrange
        var message = "Test exception";

        // Act
        Action act = () => throw new IdentityDomainException(message);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage(message);
    }

    [Fact]
    public void IdentityDomainException_CanBeCaughtAsException()
    {
        // Arrange
        var message = "Test exception";

        // Act
        Action act = () => throw new IdentityDomainException(message);

        // Assert
        act.Should().Throw<Exception>()
            .Which.Should().BeOfType<IdentityDomainException>();
    }

    [Theory]
    [InlineData("Email is required")]
    [InlineData("Invalid email format")]
    [InlineData("Password hash is required")]
    [InlineData("First name is required")]
    public void IdentityDomainException_WithValidationMessages_ShouldPreserveMessage(string validationMessage)
    {
        // Act
        var exception = new IdentityDomainException(validationMessage);

        // Assert
        exception.Message.Should().Be(validationMessage);
    }

    [Fact]
    public void Constructor_WithInnerException_ShouldPreserveStackTrace()
    {
        // Arrange
        var innerException = new ArgumentException("Inner error");
        innerException.Data.Add("TestKey", "TestValue");

        // Act
        var exception = new IdentityDomainException("Outer error", innerException);

        // Assert
        exception.InnerException.Should().BeSameAs(innerException);
        exception.InnerException!.Data["TestKey"].Should().Be("TestValue");
    }
}
