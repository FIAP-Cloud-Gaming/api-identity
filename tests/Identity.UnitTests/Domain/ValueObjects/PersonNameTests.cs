namespace Identity.UnitTests.Domain.ValueObjects;

public sealed class PersonNameTests
{
    [Fact]
    public void PersonName_WithValidNames_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var name = new PersonName("John", "Doe");

        // Assert
        name.FirstName.Should().Be("John");
        name.LastName.Should().Be("Doe");
        name.FullName.Should().Be("John Doe");
    }

    [Theory]
    [InlineData(null, "Doe")]
    [InlineData("", "Doe")]
    [InlineData("   ", "Doe")]
    [InlineData("\t", "Doe")]
    public void PersonName_WithNullOrWhitespaceFirstName_ShouldThrowIdentityDomainException(string? invalidFirstName, string lastName)
    {
        // Arrange & Act
        var act = () => new PersonName(invalidFirstName!, lastName);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("First name is required");
    }

    [Theory]
    [InlineData("John", null)]
    [InlineData("John", "")]
    [InlineData("John", "   ")]
    [InlineData("John", "\t")]
    public void PersonName_WithNullOrWhitespaceLastName_ShouldThrowIdentityDomainException(string firstName, string? invalidLastName)
    {
        // Arrange & Act
        var act = () => new PersonName(firstName, invalidLastName!);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("Last name is required");
    }

    [Theory]
    [InlineData("  John  ", "  Doe  ", "John", "Doe")]
    [InlineData("\tJohn\t", "\tDoe\t", "John", "Doe")]
    [InlineData(" John", "Doe ", "John", "Doe")]
    [InlineData("John ", " Doe", "John", "Doe")]
    public void PersonName_ShouldTrimWhitespace(string inputFirst, string inputLast, string expectedFirst, string expectedLast)
    {
        // Arrange & Act
        var name = new PersonName(inputFirst, inputLast);

        // Assert
        name.FirstName.Should().Be(expectedFirst);
        name.LastName.Should().Be(expectedLast);
    }

    [Fact]
    public void PersonName_WithFirstNameExceeding100Characters_ShouldThrowIdentityDomainException()
    {
        // Arrange
        var longFirstName = new string('A', 101);

        // Act
        var act = () => new PersonName(longFirstName, "Doe");

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("First name too long");
    }

    [Fact]
    public void PersonName_WithLastNameExceeding100Characters_ShouldThrowIdentityDomainException()
    {
        // Arrange
        var longLastName = new string('Z', 101);

        // Act
        var act = () => new PersonName("John", longLastName);

        // Assert
        act.Should().Throw<IdentityDomainException>()
            .WithMessage("Last name too long");
    }

    [Fact]
    public void PersonName_WithExactly100Characters_ShouldBeValid()
    {
        // Arrange
        var firstName = new string('A', 100);
        var lastName = new string('Z', 100);

        // Act
        var act = () => new PersonName(firstName, lastName);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void PersonName_FullName_ShouldCombineFirstAndLastName()
    {
        // Arrange
        var name = new PersonName("Jane", "Smith");

        // Act
        var fullName = name.FullName;

        // Assert
        fullName.Should().Be("Jane Smith");
    }

    [Fact]
    public void PersonName_ShouldSupportEqualityComparison()
    {
        // Arrange
        var name1 = new PersonName("John", "Doe");
        var name2 = new PersonName("John", "Doe");
        var name3 = new PersonName("Jane", "Doe");

        // Assert
        name1.Should().Be(name2);
        name1.Should().NotBe(name3);
        (name1 == name2).Should().BeTrue();
        (name1 == name3).Should().BeFalse();
    }

    [Fact]
    public void PersonName_ToString_ShouldReturnFullName()
    {
        // Arrange
        var name = new PersonName("John", "Doe");

        // Act
        var result = name.ToString();

        // Assert
        result.Should().Be("John Doe");
    }

    [Theory]
    [InlineData("John", "Doe", "John Doe")]
    [InlineData("Maria", "Silva", "Maria Silva")]
    [InlineData("A", "B", "A B")]
    public void PersonName_FullName_ShouldFormatCorrectly(string firstName, string lastName, string expectedFullName)
    {
        // Arrange
        var name = new PersonName(firstName, lastName);

        // Act & Assert
        name.FullName.Should().Be(expectedFullName);
    }
}
