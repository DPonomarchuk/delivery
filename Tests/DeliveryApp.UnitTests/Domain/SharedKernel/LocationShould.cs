using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.SharedKernel;

public class LocationShould
{
    [Fact]
    public void BeCorrectWhenParamsIsCorrectOnCreate()
    {
        //Arrange

        //Act
        var location = Location.Create(1, 3);

        //Assert
        location.IsSuccess.Should().BeTrue();
        location.Value.X.Should().Be(1);
        location.Value.Y.Should().Be(3);
    }

    [Theory]
    [InlineData(-1, 3)]
    [InlineData(2, 0)]
    [InlineData(-2, -8)]
    [InlineData(11, 8)]
    [InlineData(8, 11)]
    public void ReturnErrorWhenParamsIncorrectOnCreate(short x, short y)
    {
        //Arrange

        //Act
        var location = Location.Create(x, y);

        //Assert
        location.IsSuccess.Should().BeFalse();
        location.Error.Should().NotBeNull();
    }

    [Fact]
    public void BeEqualWhenParamsAreEqual()
    {
        //Arrange
        var location1 = Location.Create(1, 3);
        var location2 = Location.Create(1, 3);

        //Act
        var result = location1.Value == location2.Value;

        //Assert
        result.Should().BeTrue();
        location1.Value.Equals(location2.Value).Should().BeTrue();
    }

    [Fact]
    public void BeNotEqualWhenAllParamsAreEqual()
    {
        //Arrange
        var location1 = Location.Create(1, 3);
        var location2 = Location.Create(1, 3);

        //Act
        var result = location1.Value != location2.Value;

        //Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void BeCorrectWhenCreateRandom()
    {
        //Arrange

        //Act
        var location = Location.CreateRandom();

        //Assert
        location.X.Should().BeGreaterThan(0).And.BeLessThan(11);
        location.Y.Should().BeGreaterThan(0).And.BeLessThan(11);
    }

    [Theory]
    [InlineData(1, 3, 2)]
    [InlineData(4, 8, 10)]
    [InlineData(9, 9, 16)]
    public void BeCorrectWhenCalculateDistance(short x, short y, short distance)
    {
        //Arrange
        var start = Location.Create(1, 1);
        var finish = Location.Create(x, y);

        //Act
        var calculatedDistance = start.Value.CalculateDistance(finish.Value);

        //Assert
        calculatedDistance.Should().Be(distance);
    }
}