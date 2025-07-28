using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

/// <summary>
/// Место хранения заказа
/// </summary>
public class StoragePlace : Entity<Guid>
{
    /// <summary>
    /// Конструктор
    /// </summary>
    [ExcludeFromCodeCoverage]
    private StoragePlace()
    {

    }

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="name"></param>
    /// <param name="totalVolume"></param>
    [ExcludeFromCodeCoverage]
    private StoragePlace(string name, int totalVolume)
    {
        Id = Guid.NewGuid();
        Name = name;
        TotalVolume = totalVolume;
    }

    /// <summary>
    /// Название места хранения
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Допустимый объем
    /// </summary>
    public int TotalVolume { get; private set; }

    /// <summary>
    /// Идентификатор заказа
    /// </summary>
    public Guid? OrderId { get; private set; }

    /// <summary>
    /// Создание места хранения
    /// </summary>
    /// <param name="name">Название</param>
    /// <param name="totalVolume">Допустимый объем</param>
    /// <returns>Место хранения</returns>
    public static Result<StoragePlace,Error> Create(string name, int totalVolume)
    {
        if (string.IsNullOrWhiteSpace(name)) return GeneralErrors.ValueIsRequired(nameof(name));
        if (totalVolume <= 0) return GeneralErrors.ValueIsInvalid(nameof(totalVolume));

        return new StoragePlace(name, totalVolume);
    }

    /// <summary>
    /// Создать место хранения "Сумка"
    /// </summary>
    /// <returns></returns>
    public static StoragePlace CreateBag()
    {
        return new StoragePlace("Сумка", 10);
    }

    /// <summary>
    /// Проверка возможности добавления заказа
    /// </summary>
    /// <param name="volume">Объем заказа</param>
    /// <returns>Ошибки добавления заказа</returns>
    public UnitResult<Error> CanStore(int volume)
    {
        if (TotalVolume < volume) return Errors.VolumeOverLimit(TotalVolume);
        if (OrderId is not null) return Errors.OrderIsNotEmpty(OrderId.Value);

        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// Добавление заказа в место хранения
    /// </summary>
    /// <param name="orderUid">Идентификатор заказа</param>
    /// <param name="volume">Объем заказа</param>
    /// <returns>Ошибки добавления заказа</returns>
    public UnitResult<Error> Store(Guid orderUid, int volume)
    {
        var validate = CanStore(volume);
        if (!validate.IsSuccess) return validate.Error;

        OrderId = orderUid;

        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// Извлечь заказ из места хранения.
    /// </summary>
    public void Clear()
    {
        OrderId = null;
    }

    /// <summary>
    /// Ошибки, возвращаемые сущностью
    /// </summary>
    private static class Errors
    {
        public static Error VolumeOverLimit(int maxVolume)
        {
            return new Error($"{nameof(TotalVolume).ToLowerInvariant()}",
                $"Объем заказа не должен превышать {maxVolume}");
        }

        public static Error OrderIsNotEmpty(Guid order)
        {
            return new Error($"{nameof(OrderId).ToLowerInvariant()}",
                $"Место хранения занято заказом с Id: {order}");
        }
    }
}