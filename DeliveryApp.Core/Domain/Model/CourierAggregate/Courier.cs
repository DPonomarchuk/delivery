using DeliveryApp.Core.Domain.SharedKernel;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

public class Courier : Aggregate<Guid>
{
    private Courier()
    {
    }

    private Courier(string name, int speed)
    {
        Id = Guid.NewGuid();
        Name = name;
        Speed = speed;
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



}