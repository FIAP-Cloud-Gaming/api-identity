namespace Identity.UnitTests.Domain.ValueObjects;

public sealed class EmailTests
{
    [Fact]
    public void Email_WithValidEmail_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var email = new Email("test@example.com");

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData("TEST@EXAMPLE.COM", "test@example.com")]
    [InlineData("Test@Example.Com", "test@example.com")]
    [InlineData("TeSt@ExAmPlE.cOm", "test@example.com")]
    public void Email_ShouldNormalizeToLowercase(string input, string expected)
    {
        // Arrange & Act
        var email = new Email(input);

        // Assert
        email.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("  test@example.com  ", "test@example.com")]
    [InlineData("\ttest@example.com\t", "test@example.com")]
    [InlineData(" test@example.com", "test@example.com")]
    [InlineData("test@example.com ", "test@example.com")]
    public void Email_ShouldTrimWhitespace(string input, string expected)
    {
        // Arrange & Act
        var email = new Email(input);

        // Assert
        email.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Email_WithNullOrWhitespace_ShouldThrowIdentityDomainException(string? invalidEmail)
    {
        // Arrange & Act
        var act = () => new Email(invalidEmail!);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("Email is required");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    [InlineData("invalid.com")]
    [InlineData("invalid @example.com")]
    [InlineData("invalid@.com")]
    public void Email_WithInvalidFormat_ShouldThrowIdentityDomainException(string invalidEmail)
    {
        // Arrange & Act
        var act = () => new Email(invalidEmail);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("*Invalid email format*");
    }

    [Fact]
    public void Email_ShouldSupportEqualityComparison()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("test@example.com");
        var email3 = new Email("other@example.com");

        // Assert
        email1.Should().Be(email2);
        email1.Should().NotBe(email3);
        (email1 == email2).Should().BeTrue();
        (email1 == email3).Should().BeFalse();
    }

    [Fact]
    public void Email_ShouldSupportImplicitConversionToString()
    {
        // Arrange
        var email = new Email("test@example.com");

        // Act
        string emailString = email;

        // Assert
        emailString.Should().Be("test@example.com");
    }

    [Fact]
    public void Email_ToString_ShouldReturnValue()
    {
        // Arrange
        var email = new Email("test@example.com");

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void Email_WithCaseAndWhitespace_ShouldNormalizeCorrectly()
    {
        // Arrange & Act
        var email = new Email("  TEST@EXAMPLE.COM  ");

        // Assert
        email.Value.Should().Be("test@example.com");
    }
}
