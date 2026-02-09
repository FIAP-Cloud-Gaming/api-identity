namespace Identity.UnitTests.Domain.Enums;

public sealed class UserStatusTests
{
    [Fact]
    public void UserStatus_Active_ShouldHaveValue1()
    {
        // Act
        var status = UserStatus.Active;

        // Assert
        ((int)status).Should().Be(1);
    }

    [Fact]
    public void UserStatus_Inactive_ShouldHaveValue2()
    {
        // Act
        var status = UserStatus.Inactive;

        // Assert
        ((int)status).Should().Be(2);
    }

    [Fact]
    public void UserStatus_Suspended_ShouldHaveValue3()
    {
        // Act
        var status = UserStatus.Suspended;

        // Assert
        ((int)status).Should().Be(3);
    }

    [Fact]
    public void UserStatus_PendingVerification_ShouldHaveValue4()
    {
        // Act
        var status = UserStatus.PendingVerification;

        // Assert
        ((int)status).Should().Be(4);
    }

    [Fact]
    public void UserStatus_ShouldHaveExactly4Values()
    {
        // Act
        var values = Enum.GetValues<UserStatus>();

        // Assert
        values.Should().HaveCount(4);
    }

    [Fact]
    public void UserStatus_ShouldContainAllExpectedValues()
    {
        // Act
        var values = Enum.GetValues<UserStatus>();

        // Assert
        values.Should().Contain(UserStatus.Active);
        values.Should().Contain(UserStatus.Inactive);
        values.Should().Contain(UserStatus.Suspended);
        values.Should().Contain(UserStatus.PendingVerification);
    }

    [Theory]
    [InlineData(UserStatus.Active, "Active")]
    [InlineData(UserStatus.Inactive, "Inactive")]
    [InlineData(UserStatus.Suspended, "Suspended")]
    [InlineData(UserStatus.PendingVerification, "PendingVerification")]
    public void UserStatus_ToString_ShouldReturnCorrectName(UserStatus status, string expectedName)
    {
        // Act
        var name = status.ToString();

        // Assert
        name.Should().Be(expectedName);
    }

    [Theory]
    [InlineData(1, UserStatus.Active)]
    [InlineData(2, UserStatus.Inactive)]
    [InlineData(3, UserStatus.Suspended)]
    [InlineData(4, UserStatus.PendingVerification)]
    public void UserStatus_CastFromInt_ShouldReturnCorrectEnum(int value, UserStatus expected)
    {
        // Act
        var status = (UserStatus)value;

        // Assert
        status.Should().Be(expected);
    }

    [Theory]
    [InlineData(UserStatus.Active, 1)]
    [InlineData(UserStatus.Inactive, 2)]
    [InlineData(UserStatus.Suspended, 3)]
    [InlineData(UserStatus.PendingVerification, 4)]
    public void UserStatus_CastToInt_ShouldReturnCorrectValue(UserStatus status, int expected)
    {
        // Act
        var value = (int)status;

        // Assert
        value.Should().Be(expected);
    }

    [Fact]
    public void UserStatus_ShouldBeEnum()
    {
        // Assert
        typeof(UserStatus).IsEnum.Should().BeTrue();
    }

    [Fact]
    public void UserStatus_ShouldHaveJsonConverterAttribute()
    {
        // Act
        var attributes = typeof(UserStatus).GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonConverterAttribute), false);

        // Assert
        attributes.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("Active", UserStatus.Active)]
    [InlineData("Inactive", UserStatus.Inactive)]
    [InlineData("Suspended", UserStatus.Suspended)]
    [InlineData("PendingVerification", UserStatus.PendingVerification)]
    public void UserStatus_Parse_ShouldParseCorrectly(string statusName, UserStatus expected)
    {
        // Act
        var status = Enum.Parse<UserStatus>(statusName);

        // Assert
        status.Should().Be(expected);
    }

    [Fact]
    public void UserStatus_Values_ShouldBeSequential()
    {
        // Arrange
        var values = Enum.GetValues<UserStatus>().Cast<int>().OrderBy(x => x).ToList();

        // Assert
        values.Should().Equal(1, 2, 3, 4);
    }

    [Fact]
    public void UserStatus_ShouldSupportEquality()
    {
        // Arrange
        var status1 = UserStatus.Active;
        var status2 = UserStatus.Active;
        var status3 = UserStatus.Inactive;

        // Assert
        status1.Should().Be(status2);
        status1.Should().NotBe(status3);
        (status1 == status2).Should().BeTrue();
        (status1 == status3).Should().BeFalse();
    }
}
