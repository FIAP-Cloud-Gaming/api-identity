namespace Identity.UnitTests.Infrastructure.IntegrationEvents;

public sealed class EventStateEnumTests
{
    [Fact]
    public void NotPublished_ShouldHaveValueZero()
    {
        // Assert
        EventStateEnum.NotPublished.Should().Be((EventStateEnum)0);
        ((int)EventStateEnum.NotPublished).Should().Be(0);
    }

    [Fact]
    public void InProgress_ShouldHaveValueOne()
    {
        // Assert
        EventStateEnum.InProgress.Should().Be((EventStateEnum)1);
        ((int)EventStateEnum.InProgress).Should().Be(1);
    }

    [Fact]
    public void Published_ShouldHaveValueTwo()
    {
        // Assert
        EventStateEnum.Published.Should().Be((EventStateEnum)2);
        ((int)EventStateEnum.Published).Should().Be(2);
    }

    [Fact]
    public void PublishedFailed_ShouldHaveValueThree()
    {
        // Assert
        EventStateEnum.PublishedFailed.Should().Be((EventStateEnum)3);
        ((int)EventStateEnum.PublishedFailed).Should().Be(3);
    }

    [Theory]
    [InlineData(EventStateEnum.NotPublished, "NotPublished")]
    [InlineData(EventStateEnum.InProgress, "InProgress")]
    [InlineData(EventStateEnum.Published, "Published")]
    [InlineData(EventStateEnum.PublishedFailed, "PublishedFailed")]
    public void EventStateEnum_ShouldHaveCorrectNames(EventStateEnum state, string expectedName)
    {
        // Act
        var name = state.ToString();

        // Assert
        name.Should().Be(expectedName);
    }

    [Fact]
    public void EventStateEnum_ShouldBeComparable()
    {
        // Arrange
        var state1 = EventStateEnum.NotPublished;
        var state2 = EventStateEnum.InProgress;

        // Assert
        (state1 < state2).Should().BeTrue();
        (state2 > state1).Should().BeTrue();
    }

    [Fact]
    public void EventStateEnum_ShouldSupportEquality()
    {
        // Arrange
        var state1 = EventStateEnum.Published;
        var state2 = EventStateEnum.Published;
        var state3 = EventStateEnum.PublishedFailed;

        // Assert
        state1.Should().Be(state2);
        state1.Should().NotBe(state3);
    }
}
