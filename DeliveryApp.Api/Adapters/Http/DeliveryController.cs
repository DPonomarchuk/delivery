using DeliveryApp.Core.Application.Commands.CreateOrder;
using DeliveryApp.Core.Application.Queries.GetBusyCouriers;
using DeliveryApp.Core.Application.Queries.GetCreatedAndAssignedOrders;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenApi.Controllers;
using OpenApi.Models;
using Courier = OpenApi.Models.Courier;
using Location = OpenApi.Models.Location;

namespace DeliveryApp.Api.Adapters.Http;

public class DeliveryController : DefaultApiController
{
    private readonly IMediator _mediator;

    public DeliveryController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));;
    }

    public override async Task<IActionResult> CreateCourier(NewCourier newCourier)
    {
        throw new NotImplementedException();
    }

    public override async Task<IActionResult> CreateOrder()
    {
        var orderId =  Guid.NewGuid();
        var streetName = "Колотушкина";
        
        var createOrderCommand = CreateOrderCommand.Create(orderId, streetName, 5);
        if(createOrderCommand.IsFailure) return BadRequest(createOrderCommand.Error);
        
        var result = await _mediator.Send(createOrderCommand.Value);
        if (result.IsSuccess) return Ok();
        
        return BadRequest(result.Error);
    }

    public async override Task<IActionResult> GetCouriers()
    {
        var createGetBusyCouriersCommand = new GetBusyCouriersCommand();
        var commandResult = await _mediator.Send(createGetBusyCouriersCommand);
        if (!commandResult.HasValue) return NotFound();
        
        var result = commandResult.Value.Couriers.Select(c => new Courier
        {
            Id = c.Id,
            Name = c.Name,
            Location = new Location { X = c.Location.X, Y = c.Location.Y }
        });
        return Ok(result);
    }

    public override async Task<IActionResult> GetOrders()
    {
        var getOrdersCommand = new GetCreatedAndAssignedOrdersCommand();
        var commandResult = await _mediator.Send(getOrdersCommand);
        if (!commandResult.HasValue) return NotFound();

        var result = commandResult.Value.Orders.Select(o => new Order
        {
            Id = o.Id,
            Location = new Location { X = o.Location.X, Y = o.Location.Y }
        });

        return Ok(result);
    }
}