using System;
using System.Collections.Generic;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Domain.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Services;

public class DispatchServiceShould
{
    [Fact]
    public void CorrectReturnFastestCourierWhenParamsIsCorrect()
    {
        //Arrange
        var order = Order.Create(Guid.NewGuid(), Location.Create(1, 1).Value, 11).Value;
        var courier1 = Courier.Create("Тамара", 3, Location.Create(2, 4).Value).Value;
        var courier2 = Courier.Create("Антон", 3 , Location.Create(7, 5).Value).Value;
        courier2.AddStoragePlace("Багажник", 15);
        var courier3 = Courier.Create("Всесилий", 3 , Location.Create(9, 9).Value).Value;
        courier3.AddStoragePlace("Корзина", 12);
        var courier4 = Courier.Create("Улит", 1 , Location.Create(4, 4).Value).Value;
        courier4.AddStoragePlace("Корзина", 12);
        List<Courier> couriers = [courier1, courier2, courier3, courier4];

        //Act
        var dispatchService = new DispatchService();
        var courier = dispatchService.Dispatch(order, couriers);

        //Assert
        courier.IsSuccess.Should().BeTrue();
        courier.Value.Should().Be(courier2);
    }

    [Fact]
    public void ReturnErrorWhenOrderIsNull()
    {
        //Arrange
        var courier1 = Courier.Create("Тамара", 3, Location.Create(2, 4).Value).Value;
        var courier2 = Courier.Create("Антон", 3 , Location.Create(7, 5).Value).Value;
        List<Courier> couriers = [courier1, courier2];

        //Act
        var dispatchService = new DispatchService();
        var courier = dispatchService.Dispatch(null, couriers);

        //Assert
        courier.IsSuccess.Should().BeFalse();
        courier.Error.Should().NotBeNull();
    }

    [Fact]
    public void ReturnErrorWhenCouriersIsNull()
    {
        //Arrange
        var order = Order.Create(Guid.NewGuid(), Location.Create(1, 1).Value, 9).Value;

        //Act
        var dispatchService = new DispatchService();
        var courier = dispatchService.Dispatch(order, null);

        //Assert
        courier.IsSuccess.Should().BeFalse();
        courier.Error.Should().NotBeNull();
    }

    [Fact]
    public void ReturnErrorWhenOrderStatusIsNotCreated()
    {
        //Arrange
        var order = Order.Create(Guid.NewGuid(), Location.Create(1, 1).Value, 9).Value;
        var courier1 = Courier.Create("Тамара", 3, Location.Create(2, 4).Value).Value;
        order.Assign(courier1);
        var courier2 = Courier.Create("Антон", 3 , Location.Create(7, 5).Value).Value;
        List<Courier> couriers = [courier1, courier2];

        //Act
        var dispatchService = new DispatchService();
        var courier = dispatchService.Dispatch(order, couriers);

        //Assert
        courier.IsSuccess.Should().BeFalse();
        courier.Error.Should().Be(DispatchService.Errors.OrderMustBeInStatusCreated());
    }

    [Fact]
    public void ReturnErrorWhenNoOneCourierCantTakeTheOrder()
    {
        //Arrange
        var order = Order.Create(Guid.NewGuid(), Location.Create(1, 1).Value, 15).Value;
        var courier1 = Courier.Create("Тамара", 3, Location.Create(2, 4).Value).Value;
        var courier2 = Courier.Create("Антон", 3 , Location.Create(7, 5).Value).Value;
        List<Courier> couriers = [courier1, courier2];

        //Act
        var dispatchService = new DispatchService();
        var courier = dispatchService.Dispatch(order, couriers);

        //Assert
        courier.IsSuccess.Should().BeFalse();
        courier.Error.Should().Be(DispatchService.Errors.NoOneCourierHasEnoughSpace());
    }
}