using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

/// <summary>
/// Курьер
/// </summary>
public class Courier : Aggregate<Guid>
{
    /// <summary>
    /// Конструктор
    /// </summary>
    [ExcludeFromCodeCoverage]
    private Courier()
    {
    }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="speed">Скорость</param>
    /// <param name="location">Координата</param>
    [ExcludeFromCodeCoverage]
    private Courier(string name, int speed, Location location)
    {
        Id = Guid.NewGuid();
        Name = name;
        Speed = speed;
        Location = location;
        StoragePlaces = [];
    }

    /// <summary>
    /// Создание курьера
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="speed">Скорость</param>
    /// <param name="location">Расположение</param>
    /// <returns></returns>
    public static Result<Courier, Error> Create(string name, int speed, Location location)
    {
        if (string.IsNullOrEmpty(name)) return GeneralErrors.ValueIsRequired(nameof(name));
        if (speed <= 0) return GeneralErrors.ValueIsInvalid(nameof(speed));
        if (location is null) return GeneralErrors.ValueIsRequired(nameof(location));

        var courier = new Courier(name, speed, location);
        courier.AddStoragePlace("Сумка", 10);
        return courier;
    }

    /// <summary>
    /// Добавления места хранения
    /// </summary>
    /// <param name="name">Название</param>
    /// <param name="volume">Объем</param>
    /// <returns>Результат операции</returns>
    public UnitResult<Error> AddStoragePlace(string name, int volume)
    {
        var storagePlace = StoragePlace.Create(name, volume);
        if (storagePlace.IsFailure)
        {
            return storagePlace.Error;
        }

        StoragePlaces.Add(storagePlace.Value);
        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// Проверить возможность взятия заказа.
    /// </summary>
    /// <param name="order">Заказ</param>
    /// <returns></returns>
    public Result<bool, Error> CanTakeOrder(Order order)
    {
        if (order is null) return GeneralErrors.ValueIsRequired(nameof(order));
        return StoragePlaces.Any(x => x.TotalVolume >= order.Volume);
    }

    /// <summary>
    /// Взять заказ.
    /// </summary>
    /// <param name="order">Заказ</param>
    /// <returns>Результат операции</returns>
    public UnitResult<Error> TakeOrder(Order order)
    {
        var canTake = CanTakeOrder(order);
        if (canTake.IsFailure)
        {
            return canTake.Error;
        }

        if (canTake.Value is false)
        {
            return Errors.StorageOverage();
        }

        return StoragePlaces.OrderBy(x => x.TotalVolume)
            .First(x => x.TotalVolume >= order.Volume).Store(order.Id, order.Volume);
    }

    /// <summary>
    /// Завершить заказ.
    /// </summary>
    /// <param name="order">Заказ</param>
    /// <returns>Результат операции</returns>
    public UnitResult<Error> CompleteOrder(Order order)
    {
        if (order is null) return GeneralErrors.ValueIsRequired(nameof(order));

        var storagePlace = StoragePlaces.FirstOrDefault(x => x.OrderId == order.Id);
        if (storagePlace is null) return Errors.OrderNotFound(order.Id);
        storagePlace.Clear();
        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// Рассчитать количество шагов до целевого местоположения
    /// </summary>
    /// <param name="target">Координата местоположения заказа</param>
    /// <returns>Количество шагов</returns>
    public Result<double, Error> CalculateTimeToLocation(Location target)
    {
        if(target is null) return GeneralErrors.ValueIsRequired(nameof(target));

        var distance = Location.CalculateDistance(target);
        return (double)distance / Speed;
    }

    /// <summary>
    /// Изменить местоположение
    /// </summary>
    /// <param name="target">Целевое местоположение</param>
    /// <returns>Местоположение после сдвига</returns>
    public UnitResult<Error> Move(Location target)
    {
        if (target == null) return GeneralErrors.ValueIsRequired(nameof(target));

        var difX = target.X - Location.X;
        var difY = target.Y - Location.Y;
        var cruisingRange = Speed;

        var moveX = Math.Clamp(difX, -cruisingRange, cruisingRange);
        cruisingRange -= Math.Abs(moveX);

        var moveY = Math.Clamp(difY, -cruisingRange, cruisingRange);

        var locationCreateResult = Location.Create((short)(Location.X + moveX), (short)(Location.Y + moveY));
        if (locationCreateResult.IsFailure) return locationCreateResult.Error;
        Location = locationCreateResult.Value;

        return UnitResult.Success<Error>();
    }


    /// <summary>
    /// Имя
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Скорость
    /// </summary>
    public int Speed { get; private set; }

    /// <summary>
    /// Местоположение
    /// </summary>
    public Location Location { get; private set; }

    /// <summary>
    /// Места для хранения
    /// </summary>
    public List<StoragePlace> StoragePlaces { get; private set; }

    /// <summary>
    /// Ошибки, которые может возвращать сущность
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error StorageOverage()
        {
            return new Error($"{nameof(StoragePlaces).ToLowerInvariant()}.cant.take.order",
                "Невозможно взять заказ, объем заказа превышает допустимый объем в местах хранения");
        }

        public static Error OrderNotFound(Guid orderId)
        {
            return new Error($"{nameof(StoragePlaces).ToLowerInvariant()}.order.not.found",
                $"Невозможно завершить заказ, в местах хранения заказа не найден заказ с номером '{orderId}'");
        }
    }
}