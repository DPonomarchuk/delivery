namespace DeliveryApp.Core.Domain.Model.OrderAggregate;

using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;

/// <summary>
///     Статус заказа
/// </summary>
public class OrderStatus : ValueObject
{
    /// <summary>
    ///     Ctr
    /// </summary>
    [ExcludeFromCodeCoverage]
    private OrderStatus()
    {
    }

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <param name="name">Название</param>
    private OrderStatus(string name) : this()
    {
        Name = name;
    }

    public static OrderStatus Created => new(nameof(Created).ToLowerInvariant());
    public static OrderStatus Assigned => new(nameof(Assigned).ToLowerInvariant());
    public static OrderStatus Completed => new(nameof(Completed).ToLowerInvariant());

    /// <summary>
    ///     Название
    /// </summary>
    public string Name { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}
