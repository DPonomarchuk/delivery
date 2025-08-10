using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Commands.AssignOrder;

public class AssignOrderHandler : IRequestHandler<AssignOrderCommand, UnitResult<Error>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICourierRepository _courierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDispatchService _dispatchService;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="orderRepository">Репозиторий заказов</param>
    /// <param name="unitOfWork">ЮОВ</param>
    /// <param name="courierRepository">Репозиторий курьеров</param>
    /// <param name="dispatchService">Сервис распределения заказов на курьеров</param>
    public AssignOrderHandler(IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICourierRepository courierRepository,
        IDispatchService dispatchService)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _courierRepository = courierRepository;
        _dispatchService = dispatchService;
    }
    
    public async Task<UnitResult<Error>> Handle(AssignOrderCommand request, CancellationToken cancellationToken)
    {
        var orderResult = await _orderRepository.GetFirstInCreatedStatusAsync();
        if (orderResult.HasNoValue) return Errors.NotAvailableOrders();

        var couriers = await _courierRepository.GetAllFree();
        if (couriers.Count == 0) return Errors.NotAvailableCouriers();

        var order = orderResult.Value;
        var dispatchResult = _dispatchService.Dispatch(order, couriers);
        if (dispatchResult.IsFailure) return dispatchResult.Error;
        
        _orderRepository.Update(order);
        _courierRepository.Update(dispatchResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }
    
    /// <summary>
    /// Ошибки, возвращаемые handler'ом
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error NotAvailableOrders()
        {
            return new Error("not.available.orders", "Нет доступных заказов для распределения");
        }
        
        public static Error NotAvailableCouriers()
        {
            return new Error("not.available.couriers", "Нет доступных курьеров для взятия заказа");
        }
    }
}