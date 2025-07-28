using System;
using System.Collections.Generic;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
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
        var courier = Courier.Create("Ваня", 7, location);

        //Assert
        courier.IsSuccess.Should().BeTrue();
        courier.Value.Name.Should().Be("Ваня");
        courier.Value.Speed.Should().Be(7);
        courier.Value.Location.Should().NotBeNull();
        courier.Value.StoragePlaces.Should().HaveCount(1);
        courier.Value.StoragePlaces[0].Name.Should().Be("Сумка");
        courier.Value.StoragePlaces[0].TotalVolume.Should().Be(10);
        courier.Value.StoragePlaces.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(GetInvalidCreateData))]
    public void BeIncorrectWhenParamsIsIncorrectOnCreate(string name, int speed, Location location)
    {
        //Arrange

        //Act
        var courier = Courier.Create(name, speed, location);

        //Assert
        courier.IsSuccess.Should().BeFalse();
        courier.Error.Should().NotBeNull();
    }

    [Fact]
    public void CanAddStoragePlaceIfValueIsCorrect()
    {
        //Arrange
        var courier = Courier.Create("Ваня", 7, Location.CreateRandom()).Value;

        //Act
        var result = courier.AddStoragePlace("Корзина", 5);

        //Assert
        result.IsSuccess.Should().BeTrue();
        courier.StoragePlaces.Should().HaveCount(2);
        courier.StoragePlaces[1].Name.Should().Be("Корзина");
        courier.StoragePlaces[1].TotalVolume.Should().Be(5);
    }

    [Fact]
    public void CantAddStoragePlaceIfValueIsIncorrect()
    {
        //Arrange
        var courier = Courier.Create("Ваня", 7, Location.CreateRandom()).Value;

        //Act
        var result = courier.AddStoragePlace("", -1);

        //Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void CanCheckTakingOrderIfOrderVolumeIsCorrect()
    {
        //Arrange
        var courier = Courier.Create("Ваня", 7, Location.CreateRandom()).Value;
        courier.AddStoragePlace("Корзина", 15);
        var order = Order.Create(Guid.NewGuid(), Location.CreateRandom(), 14).Value;

        //Act
        var result = courier.CanTakeOrder(order);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public void CanCheckTakingOrderIfOrderIsOverVolume()
    {
        //Arrange
        var courier = Courier.Create("Ваня", 7, Location.CreateRandom()).Value;
        courier.AddStoragePlace("Корзина", 15);
        var order = Order.Create(Guid.NewGuid(), Location.CreateRandom(), 16).Value;

        //Act
        var result = courier.CanTakeOrder(order);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public void CantCheckTakingOrderIfOrderIsNull()
    {
        //Arrange
        var courier = Courier.Create("Ваня", 7, Location.CreateRandom()).Value;
        courier.AddStoragePlace("Корзина", 15);

        //Act
        var result = courier.CanTakeOrder(null);

        //Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void CanTakeOrderIfOrderVolumeIsCorrect()
    {
        //Arrange
        var courier = Courier.Create("Ваня", 7, Location.CreateRandom()).Value;
        var order = Order.Create(Guid.NewGuid(), Location.CreateRandom(), 9).Value;

        //Act
        var result = courier.TakeOrder(order);

        //Assert
        result.IsSuccess.Should().BeTrue();
        courier.StoragePlaces[0].OrderId.Should().Be(order.Id);
    }

    [Fact]
    public void CantTakeOrderIfOrderVolumeIsOverage()
    {
        //Arrange
        var courier = Courier.Create("Ваня", 7, Location.CreateRandom()).Value;
        var order = Order.Create(Guid.NewGuid(), Location.CreateRandom(), 16).Value;

        //Act
        var result = courier.TakeOrder(order);

        //Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Courier.Errors.StorageOverage());
    }

    [Fact]
    public void CantTakeOrderIfOrderIsNull()
    {
        //Arrange
        var courier = Courier.Create("Ваня", 7, Location.CreateRandom()).Value;

        //Act
        var result = courier.TakeOrder(null);

        //Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void CanCompleteOrderIfOrderIsCorrect()
    {
        //Arrange
        var courier = Courier.Create("Ваня", 7, Location.CreateRandom()).Value;
        var order = Order.Create(Guid.NewGuid(), Location.CreateRandom(), 10).Value;
        courier.TakeOrder(order);

        //Act
        var result = courier.CompleteOrder(order);

        //Assert
        result.IsSuccess.Should().BeTrue();
        courier.StoragePlaces[0].OrderId.Should().BeNull();
    }

    [Fact]
    public void CantCompleteOrderIfStoragePlaceOrderIsEmpty()
    {
        //Arrange
        var courier = Courier.Create("Ваня", 7, Location.CreateRandom()).Value;
        var order = Order.Create(Guid.NewGuid(), Location.CreateRandom(), 16).Value;

        //Act
        var result = courier.CompleteOrder(order);

        //Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Courier.Errors.OrderNotFound(order.Id));
    }

    [Fact]
    public void CantCompleteOrderIfOrderIsEmpty()
    {
        //Arrange
        var courier = Courier.Create("Ваня", 7, Location.CreateRandom()).Value;

        //Act
        var result = courier.CompleteOrder(null);

        //Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Theory]
    [InlineData(2, 1, 1, 10, 10, 9)]
    [InlineData(1, 8, 8, 10, 10, 4)]
    [InlineData(3, 2, 3, 7, 5, 2.3333333333333335)]
    public void CanCalculateTimeToLocationIfLocationIsCorrect(int speed, short cX, short cY, short tX, short tY, double expectedTime)
    {
        //Arrange
        var courierLocation = Location.Create(cX, cY).Value;
        var targetLocation = Location.Create(tX, tY).Value;
        var courier = Courier.Create("Ваня", speed, courierLocation).Value;

        //Act
        var result = courier.CalculateTimeToLocation(targetLocation);

        //Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedTime);
    }

    [Fact]
    public void CantCalculateTimeToLocationIfLocationIsIncorrect()
    {
        //Arrange
        var courierLocation = Location.Create(1, 2).Value;
        var courier = Courier.Create("Ваня", 5, courierLocation).Value;

        //Act
        var result = courier.CalculateTimeToLocation(null);

        //Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Theory]
    [InlineData(2, 1, 1, 10, 10, 3, 1)]
    [InlineData(1, 8, 8, 10, 10, 9, 8)]
    [InlineData(6, 6, 5, 2, 1, 2, 3)]
    public void CanMoveIfLocationIsCorrect(int speed, short cX, short cY, short tX, short tY, short mX, short mY)
    {
        //Arrange
        var courierLocation = Location.Create(cX, cY).Value;
        var targetLocation = Location.Create(tX, tY).Value;
        var moveLocation = Location.Create(mX, mY).Value;
        var courier = Courier.Create("Ваня", speed, courierLocation).Value;

        //Act
        var result = courier.Move(targetLocation);

        //Assert
        result.IsSuccess.Should().BeTrue();
        courier.Location.Should().Be(moveLocation);
    }

    [Fact]
    public void CantMoveIfLocationIsIncorrect()
    {
        //Arrange
        var courier = Courier.Create("Ваня", 2, Location.Create(1, 2).Value).Value;

        //Act
        var result = courier.Move(null);

        //Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    public static IEnumerable<object[]> GetInvalidCreateData()
    {
        yield return ["", 4, Location.Create(1, 2).Value];
        yield return ["Артём", -1, Location.Create(5, 5).Value];
        yield return ["Александр", 2, null];
    }

}