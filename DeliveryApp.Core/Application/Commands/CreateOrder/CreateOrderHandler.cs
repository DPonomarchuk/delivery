using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Commands.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, UnitResult<Error>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    /// <summary>
    ///     Ctr
    /// </summary>
    public CreateOrderHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }
    
    public async Task<UnitResult<Error>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var getOrderResult = await _orderRepository.GetAsync(request.OrderId);
        if (getOrderResult.HasValue) return UnitResult.Success<Error>();

        var location = Location.CreateRandom();

        var orderCreateResult = Order.Create(request.OrderId, location, request.Volume);
        if (orderCreateResult.IsFailure) return orderCreateResult;
        var order = orderCreateResult.Value;

        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }
    
    /// <summary>
    /// Ошибки, возвращаемые handler'ом
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error OrderIdAlreadyExists(Guid orderId)
        {
            return new Error($"{nameof(orderId).ToLowerInvariant()}.already.exists",
                $"Заказ с идентификатором {orderId} присутствует в системе");
        }
    }
}