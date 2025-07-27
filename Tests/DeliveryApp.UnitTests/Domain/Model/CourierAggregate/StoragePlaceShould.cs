using System;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate;

public class StoragePlaceShould
{
    [Fact]
    public void BeCorrectWhenParamsIsCorrectOnCreate()
    {
        //Arrange

        //Act
        var location = StoragePlace.Create("Багажник", 10);

        //Assert
        location.IsSuccess.Should().BeTrue();
        location.Value.Name.Should().Be("Багажник");
        location.Value.TotalVolume.Should().Be(10);
    }

    [Theory]
    [InlineData("", 10)]
    [InlineData(null, 10)]
    [InlineData("Сумка", 0)]
    [InlineData("Рюкзак", -11)]
    [InlineData("", -3)]
    public void ReturnErrorWhenParamsIncorrectOnCreate(string name, int volume)
    {
        //Arrange

        //Act
        var location = StoragePlace.Create(name, volume);

        //Assert
        location.IsSuccess.Should().BeFalse();
        location.Error.Should().NotBeNull();
    }

    [Fact]
    public void CanSetCorrectOrder()
    {
        //Arrange
        var storage = StoragePlace.Create("Корзина", 3).Value;
        var orderUid = Guid.NewGuid();
        //Act
        var result = storage.Store(orderUid, 3);

        //Assert
        result.IsSuccess.Should().BeTrue();
        storage.OrderId.Should().Be(orderUid);
    }

    [Fact]
    public void CanStoreIfCorrectVolume()
    {
        //Arrange
        var storage = StoragePlace.Create("Корзина", 3).Value;

        //Act
        var result = storage.CanStore(3);

        //Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CantStoreIfIncorrectVolume()
    {
        //Arrange
        var storage = StoragePlace.Create("Корзина", 3).Value;

        //Act
        var result = storage.CanStore(50);

        //Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void CantSetOrderWithIncorrectVolume()
    {
        //Arrange
        var storage = StoragePlace.Create("Корзина", 3).Value;

        //Act
        var result = storage.Store(Guid.NewGuid(), 10);

        //Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void CantSetOrderToStorageWithOrder()
    {
        //Arrange
        var storage = StoragePlace.Create("Корзина", 3).Value;
        storage.Store(Guid.NewGuid(), 2);

        //Act
        var result = storage.Store(Guid.NewGuid(), 3);

        //Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void BeCorrectWhenExtractOrder()
    {
        //Arrange
        var storage = StoragePlace.Create("Корзина", 3).Value;
        storage.Store(Guid.NewGuid(), 2);

        //Act
        storage.Clear();

        //Assert
        storage.OrderId.Should().BeNull();
    }

    [Fact]
    public void BeNotEqualWhenParamsIsEqual()
    {
        //Arrange
        var storage = StoragePlace.Create("Корзина", 3).Value;
        var storage2 = StoragePlace.Create("Корзина", 3).Value;

        //Act
        var result = storage.Equals(storage2);

        //Assert
        result.Should().BeFalse();
    }
}