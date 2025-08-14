using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Application.Commands.CreateOrder;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Core.Ports;
using FluentAssertions;
using NSubstitute;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Application;

public class CreateOrderCommandShould
{
    private readonly IOrderRepository _orderRepositoryMock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGeoClient _geoClient;
    
    public CreateOrderCommandShould()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _orderRepositoryMock = Substitute.For<IOrderRepository>();
        _geoClient = Substitute.For<IGeoClient>();
    }
    
    private Maybe<Order> ExistedOrder()
    {
        return Order.Create(Guid.NewGuid(), Location.Create(1, 1).Value,5).Value;
    }
    
    [Fact]
    public async Task ReturnTrueWhenOrderExists()
    {
        //Arrange
        _orderRepositoryMock.GetAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult(ExistedOrder()));
        _unitOfWork.SaveChangesAsync()
            .Returns(Task.FromResult(true));
        _geoClient.GetLocation(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Location.Create(2, 2)));

        //Act
        var createCreateOrderCommandResult = CreateOrderCommand.Create(Guid.NewGuid(), "Ленина",5);
        createCreateOrderCommandResult.IsSuccess.Should().BeTrue();
        var handler = new CreateOrderHandler(_unitOfWork, _orderRepositoryMock, _geoClient);
        var result = await handler.Handle(createCreateOrderCommandResult.Value, CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
    }
}