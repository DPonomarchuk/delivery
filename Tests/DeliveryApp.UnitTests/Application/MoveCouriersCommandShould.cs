using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Application.Commands.CreateOrder;
using DeliveryApp.Core.Application.Commands.MoveCouriers;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Core.Ports;
using FluentAssertions;
using NSubstitute;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Application;

public class MoveCouriersCommandShould
{
    private readonly ICourierRepository _courierRepositoryMock;
    private readonly IOrderRepository _orderRepositoryMock;
    private readonly IUnitOfWork _unitOfWork;
    
    public MoveCouriersCommandShould()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _orderRepositoryMock = Substitute.For<IOrderRepository>();
        _courierRepositoryMock = Substitute.For<ICourierRepository>();
    }
    
    [Fact]
    public async Task ReturnTrueWhenNoAssignedOrders()
    {
        //Arrange
        _orderRepositoryMock.GetAllInAssignedStatus()
            .Returns(null as IEnumerable<Order>);
        _unitOfWork.SaveChangesAsync()
            .Returns(Task.FromResult(true));
        var moveCouriersCommand = new MoveCouriersCommand();

        //Act
        var handler = new MoveCouriersHandler(_courierRepositoryMock, _unitOfWork, _orderRepositoryMock);
        var result = await handler.Handle(moveCouriersCommand, CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public async Task ReturnTrueWhenAllCourierDoMove()
    {
        //Arrange
        _orderRepositoryMock.GetAllInAssignedStatus()
            .Returns(GetAssignedOrders());
        _courierRepositoryMock.GetAsync(Arg.Any<Guid>()).Returns(GetCourier());
        _unitOfWork.SaveChangesAsync()
            .Returns(Task.FromResult(true));
        var moveCouriersCommand = new MoveCouriersCommand();

        //Act
        var handler = new MoveCouriersHandler(_courierRepositoryMock, _unitOfWork, _orderRepositoryMock);
        var result = await handler.Handle(moveCouriersCommand, CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public async Task ReturnErrorWhenCourierIsNull()
    {
        //Arrange
        _orderRepositoryMock.GetAllInAssignedStatus()
            .Returns(GetAssignedOrders());
        _courierRepositoryMock.GetAsync(Arg.Any<Guid>()).Returns(Maybe<Courier>.None);
        _unitOfWork.SaveChangesAsync()
            .Returns(Task.FromResult(true));
        var moveCouriersCommand = new MoveCouriersCommand();

        //Act
        var handler = new MoveCouriersHandler(_courierRepositoryMock, _unitOfWork, _orderRepositoryMock);
        var result = await handler.Handle(moveCouriersCommand, CancellationToken.None);

        //Assert
        result.IsFailure.Should().BeTrue();
    }
    
    private IEnumerable<Order> GetAssignedOrders()
    {
        var order1 = Order.Create(Guid.AllBitsSet, Location.Create(1, 1).Value, 5).Value;
        var courier1 = Courier.Create("Антон", 2, Location.Create(4,4).Value).Value;
        order1.Assign(courier1);
        courier1.TakeOrder(order1);
        return
        [
            order1
        ];
    }
    
    private Maybe<Courier> GetCourier()
    {
        var order1 = Order.Create(Guid.AllBitsSet, Location.Create(1, 1).Value, 5).Value;
        var courier1 = Courier.Create("Антон", 2, Location.Create(4,4).Value).Value;
        order1.Assign(courier1);
        courier1.TakeOrder(order1);
        return courier1;
    }
}