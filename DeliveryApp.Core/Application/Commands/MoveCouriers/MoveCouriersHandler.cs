using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Commands.MoveCouriers;

public class MoveCouriersHandler : IRequestHandler<MoveCouriersCommand, UnitResult<Error>>
{
    private readonly ICourierRepository _courierRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MoveCouriersHandler(ICourierRepository courierRepository, IUnitOfWork unitOfWork, IOrderRepository orderRepository)
    {
        _courierRepository = courierRepository;
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
    }

    public async Task<UnitResult<Error>> Handle(MoveCouriersCommand request, CancellationToken cancellationToken)
    {
        var assignedOrders = _orderRepository.GetAllInAssignedStatus().ToList();
        if (assignedOrders.Count == 0) return UnitResult.Success<Error>();

        foreach (var order in assignedOrders)
        {
            if (order.CourierId is null) 
                return GeneralErrors.ValueIsRequired(nameof(order.CourierId));
            
            var courierResult = await _courierRepository.GetAsync(order.CourierId.Value);
            if (courierResult.HasNoValue) return GeneralErrors.ValueIsInvalid(nameof(order.CourierId));
            var courier = courierResult.Value;
            var moveResult = courier.Move(order.Location);
            if (moveResult.IsFailure) return moveResult.Error;
            
            if (courier.Location.Equals(order.Location))
            {
                var courierCompleteOrderResult = courier.CompleteOrder(order);
                if (courierCompleteOrderResult.IsFailure) return courierCompleteOrderResult.Error;
                
                var orderCompleteResult = order.Complete();
                if (orderCompleteResult.IsFailure) return orderCompleteResult.Error;
            }
            
            _courierRepository.Update(courier);
            _orderRepository.Update(order);
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }
}