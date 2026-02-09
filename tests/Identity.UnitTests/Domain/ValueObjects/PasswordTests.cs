namespace Identity.UnitTests.Domain.ValueObjects;

public sealed class PasswordTests
{
    [Fact]
    public void FromHash_WithValidHash_ShouldCreateSuccessfully()
    {
        // Arrange
        var hash = "ABCDEF0123456789ABCDEF0123456789";

        // Act
        var password = Password.FromHash(hash);

        // Assert
        password.Hash.Should().Be(hash);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void FromHash_WithNullOrWhitespace_ShouldThrowIdentityDomainException(string? invalidHash)
    {
        // Arrange & Act
        var act = () => Password.FromHash(invalidHash!);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("Password hash is required");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void ValidateStrength_WithNullOrWhitespace_ShouldThrowIdentityDomainException(string? invalidPassword)
    {
        // Arrange & Act
        var act = () => Password.ValidateStrength(invalidPassword!);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("Password is required");
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("abc")]
    [InlineData("abcd")]
    [InlineData("abcde")]
    [InlineData("abcdef")]
    [InlineData("abcdefg")]
    public void ValidateStrength_WithLessThan8Characters_ShouldThrowIdentityDomainException(string shortPassword)
    {
        // Arrange & Act
        var act = () => Password.ValidateStrength(shortPassword);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("Password must be at least 8 characters");
    }

    [Theory]
    [InlineData("abcdefgh1")]
    [InlineData("alllowercase123")]
    [InlineData("nouppercase456")]
    public void ValidateStrength_WithoutUppercase_ShouldThrowIdentityDomainException(string password)
    {
        // Arrange & Act
        var act = () => Password.ValidateStrength(password);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("Password must contain uppercase letter");
    }

    [Theory]
    [InlineData("ABCDEFGH1")]
    [InlineData("ALLLOWERCASE123")]
    [InlineData("NOLOWERCASE456")]
    public void ValidateStrength_WithoutLowercase_ShouldThrowIdentityDomainException(string password)
    {
        // Arrange & Act
        var act = () => Password.ValidateStrength(password);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("Password must contain lowercase letter");
    }

    [Theory]
    [InlineData("Abcdefgh")]
    [InlineData("NoNumbersHere")]
    [InlineData("AnotherPassword")]
    public void ValidateStrength_WithoutDigit_ShouldThrowIdentityDomainException(string password)
    {
        // Arrange & Act
        var act = () => Password.ValidateStrength(password);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("Password must contain a number");
    }

    [Theory]
    [InlineData("Abcdefgh1")]
    [InlineData("Password123")]
    [InlineData("ValidPass1")]
    [InlineData("MyP@ssw0rd")]
    [InlineData("Test1234ABC")]
    public void ValidateStrength_WithValidPassword_ShouldNotThrow(string validPassword)
    {
        // Arrange & Act
        var act = () => Password.ValidateStrength(validPassword);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Password_ShouldSupportEqualityComparison()
    {
        // Arrange
        var password1 = Password.FromHash("hash123");
        var password2 = Password.FromHash("hash123");
        var password3 = Password.FromHash("differentHash");

        // Assert
        password1.Should().Be(password2);
        password1.Should().NotBe(password3);
        (password1 == password2).Should().BeTrue();
        (password1 == password3).Should().BeFalse();
    }

    [Fact]
    public void Password_ToString_ShouldReturnMaskedValue()
    {
        // Arrange
        var password = Password.FromHash("ABCDEF0123456789ABCDEF0123456789");

        // Act
        var result = password.ToString();

        // Assert
        result.Should().Be("********");
    }

    [Fact]
    public void Password_ToString_ShouldNotExposeHash()
    {
        // Arrange
        var hash = "SecretHash123456789";
        var password = Password.FromHash(hash);

        // Act
        var result = password.ToString();

        // Assert
        result.Should().NotContain(hash);
        result.Should().Be("********");
    }
}
