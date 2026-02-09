namespace Identity.UnitTests.Infrastructure.Services;

public sealed class PasswordHasherTests
{
    private readonly IPasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void Hash_ShouldReturnHashInCorrectFormat()
    {
        // Arrange
        const string password = "MyPassword123";

        // Act
        var hash = _passwordHasher.Hash(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().Contain("-");

        var parts = hash.Split('-');
        parts.Should().HaveCount(2);
        parts[0].Should().NotBeNullOrEmpty(); // Hash part
        parts[1].Should().NotBeNullOrEmpty(); // Salt part
    }

    [Fact]
    public void Hash_ShouldReturnDifferentHashesForSamePassword()
    {
        // Arrange
        const string password = "MyPassword123";

        // Act
        var hash1 = _passwordHasher.Hash(password);
        var hash2 = _passwordHasher.Hash(password);

        // Assert
        hash1.Should().NotBe(hash2, "different salts should produce different hashes");
    }

    [Fact]
    public void Hash_ShouldReturnHexadecimalStrings()
    {
        // Arrange
        const string password = "MyPassword123";

        // Act
        var hash = _passwordHasher.Hash(password);
        var parts = hash.Split('-');

        // Assert
        parts[0].Should().MatchRegex("^[0-9A-F]+$", "hash part should be hexadecimal");
        parts[1].Should().MatchRegex("^[0-9A-F]+$", "salt part should be hexadecimal");
    }

    [Fact]
    public void Verify_ShouldReturnTrue_WhenPasswordIsCorrect()
    {
        // Arrange
        const string password = "MyPassword123";
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenPasswordIsIncorrect()
    {
        // Arrange
        const string password = "MyPassword123";
        const string wrongPassword = "WrongPassword123";
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("Password1")]
    [InlineData("AnotherPassword2")]
    [InlineData("SuperSecret3")]
    public void Verify_ShouldReturnTrue_ForVariousValidPasswords(string password)
    {
        // Arrange
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenPasswordCaseIsDifferent()
    {
        // Arrange
        const string password = "MyPassword123";
        const string differentCase = "mypassword123";
        var hash = _passwordHasher.Hash(password);

        // Act
        var result = _passwordHasher.Verify(differentCase, hash);

        // Assert
        result.Should().BeFalse("password verification should be case-sensitive");
    }
}
