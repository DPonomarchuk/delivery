using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Domain.SharedKernel;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate;

/// <summary>
///     Заказ
/// </summary>
public sealed class Order : Aggregate<Guid>
{
    /// <summary>
    ///     Ctr
    /// </summary>
    [ExcludeFromCodeCoverage]
    private Order()
    {
    }

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <param name="orderId">Идентификатор заказа</param>
    /// <param name="location">Геопозиция</param>
    /// <param name="volume">Объем</param>
    private Order(Guid orderId, Location location, int volume) : this()
    {
        Id = orderId;
        Location = location;
        Volume = volume;
        Status = OrderStatus.Created;
    }

    /// <summary>
    ///     Местоположение, куда нужно доставить заказ
    /// </summary>
    public Location Location { get; private set; }

    /// <summary>
    ///     Объем
    /// </summary>
    public int Volume { get; private set; }

    /// <summary>
    ///     Статус
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    ///     Идентификатор исполнителя (курьера)
    /// </summary>
    public Guid? CourierId { get; private set; }

    /// <summary>
    ///     Factory Method
    /// </summary>
    /// <param name="orderId">Идентификатор заказа</param>
    /// <param name="location">Геопозиция</param>
    /// <param name="volume">Объем</param>
    /// <returns>Результат</returns>
    public static Result<Order, Error> Create(Guid orderId, Location location, int volume)
    {
        if (orderId == Guid.Empty) return GeneralErrors.ValueIsRequired(nameof(orderId));
        if (location == null) return GeneralErrors.ValueIsRequired(nameof(location));
        if (volume <= 0) return GeneralErrors.ValueIsRequired(nameof(volume));

        return new Order(orderId, location,volume);
    }

    /// <summary>
    ///     Назначить заказ на курьера
    /// </summary>
    /// <param name="courier">Курьер</param>
    /// <returns>Результат</returns>
    public UnitResult<Error> Assign(Courier courier)
    {
        if (courier == null) return GeneralErrors.ValueIsRequired(nameof(courier));
        if (Status != OrderStatus.Created) return Errors.ErrCantAssignAlreadyAssignedOrder(courier.Id);

        CourierId = courier.Id;
        Status = OrderStatus.Assigned;

        return UnitResult.Success<Error>();
    }

    /// <summary>
    ///     Завершить выполнение заказа
    /// </summary>
    /// <returns>Результат</returns>
    public UnitResult<Error> Complete()
    {
        if (Status != OrderStatus.Assigned) return Errors.ErrCantCompleteNotAssignedOrder();
        if (CourierId == null) return Errors.ErrCantCompleteNotAssignedOrder();

        Status = OrderStatus.Completed;
        RaiseDomainEvent(new OrderChangedDomainEvent(this));
        return UnitResult.Success<Error>();
    }

    /// <summary>
    ///     Ошибки, которые может возвращать сущность
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error ErrCantCompleteNotAssignedOrder()
        {
            return new Error($"{nameof(Order).ToLowerInvariant()}.cant.complete.not.assigned.order",
                "Нельзя завершить заказ, который не был назначен");
        }

        public static Error ErrCantAssignAlreadyAssignedOrder(Guid courierId)
        {
            return new Error($"{nameof(Order).ToLowerInvariant()}.cant.assign.already.assigned.order",
                $"Нельзя назначить уже назначенный заказ {courierId} ");
        }
    }
}
