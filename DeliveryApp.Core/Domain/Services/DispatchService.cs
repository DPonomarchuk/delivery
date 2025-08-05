using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Domain.Services;

/// <summary>
/// Сервис скоринга курьеров
/// </summary>
public class DispatchService : IDispatchService
{
    /// <summary>
    /// Распределение заказа на курьеров
    /// </summary>
    /// <param name="order">Заказ</param>
    /// <param name="couriers">Курьеры</param>
    /// <returns>Самый подходящий курьер</returns>
    /// <exception cref="NotImplementedException"></exception>
    public Result<Courier, Error> Dispatch(Order order, List<Courier> couriers)
    {
        if (order == null) return GeneralErrors.ValueIsRequired(nameof(order));
        if (couriers == null) return GeneralErrors.ValueIsRequired(nameof(couriers));
        if (order.Status != OrderStatus.Created) return Errors.OrderMustBeInStatusCreated();

        var possibleCouriers = couriers.Where(c => c.CanTakeOrder(order).Value).ToList();
        if (possibleCouriers.Count == 0) return Errors.NoOneCourierHasEnoughSpace();

        var courier = possibleCouriers.OrderBy(x => x.CalculateTimeToLocation(order.Location).Value).First();

        var assignResult = order.Assign(courier);
        if (!assignResult.IsSuccess) return assignResult.Error;

        var takeOrder = courier.TakeOrder(order);
        if (!takeOrder.IsSuccess) return takeOrder.Error;

        return courier;
    }

    /// <summary>
    /// Ошибки, которые может возвращать сущность
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error OrderMustBeInStatusCreated()
        {
            return new Error($"{nameof(Order).ToLowerInvariant()}.must.be.in.status.created",
                $"Заказ должен находиться в статусе '{OrderStatus.Created.Name}'");
        }

        public static Error NoOneCourierHasEnoughSpace()
        {
            return new Error($"no.one.{nameof(Courier).ToLowerInvariant()}.has.enough.space",
                "Ни один курьер из списка не имеет достаточно места для доставки заказа");
        }
    }
}