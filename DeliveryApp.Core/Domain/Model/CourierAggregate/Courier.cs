using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

public class Courier : Aggregate<Guid>
{
    private Courier()
    {
    }

    private Courier(string name, int speed, Location location)
    {
        Id = Guid.NewGuid();
        Name = name;
        Speed = speed;
        Location = location;
    }

    /// <summary>
    ///     Создание курьера
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="speed">Скорость</param>
    /// <param name="location">Расположение</param>
    /// <returns></returns>
    public static Result<Courier, Error> Create(string name, int speed, Location location)
    {
        // validations
        var courier = new Courier(name, speed, location);

        var bag = StoragePlace.Create("Сумка", 10).Value;
        courier.AddStoragePlace(bag);
        return courier;
    }

    /// <summary>
    /// Добавления места хранения
    /// </summary>
    /// <param name="storagePlace">Место хранения</param>
    /// <returns>Результат операции</returns>
    public UnitResult<Error> AddStoragePlace(StoragePlace storagePlace)
    {
        StoragePlaces.Add(storagePlace);
        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// Проверить возможность взятия заказа.
    /// </summary>
    /// <param name="order">Заказ</param>
    /// <returns></returns>
    public Result<bool, Error> CanTakeOrder(Order order)
    {
        if (StoragePlaces is null || StoragePlaces.Count < 1) return Errors.NoStoragePlaces();
        return StoragePlaces.Any(x => x.TotalVolume >= order.Volume);
    }

    /// <summary>
    ///     Имя
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    ///     Скорость
    /// </summary>
    public int Speed { get; private set; }

    /// <summary>
    ///     Местоположение
    /// </summary>
    public Location Location { get; private set; }

    /// <summary>
    /// Места для хранения
    /// </summary>
    public List<StoragePlace> StoragePlaces { get; private set; }

    /// <summary>
    ///     Ошибки, которые может возвращать сущность
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error NoStoragePlaces()
        {
            return new Error($"{nameof(StoragePlaces).ToLowerInvariant()}.cant.take.order",
                "Невозможно взять заказ, нет доступных мест для хранения");
        }
    }
}