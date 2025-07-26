using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate;

public class CourierShould
{
    [Fact]
    public void BeCorrectWhenParamsIsCorrectOnCreate()
    {
        //Arrange
        var location = Location.CreateRandom();
        //Act
        var courier = Courier.Create("Ваня", 10, location);

        //Assert
        courier.IsSuccess.Should().BeTrue();
        courier.Value.Name.Should().Be("Ваня");
        courier.Value.Speed.Should().Be(10);
        courier.Value.Location.Should().NotBeNull();
        courier.Value.StoragePlaces.Should().NotBeNull();
    }
}