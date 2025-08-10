namespace DeliveryApp.Core.Application.UseCases.Queries.GetCreatedAndAssignedOrders;

public class GetCreatedAndAssignedOrdersResponse
{
    public GetCreatedAndAssignedOrdersResponse(List<Order> orders)
    {
        Orders.AddRange(orders);
    }

    public List<Order> Orders { get; set; } = new();
}

public class Order
{
    /// <summary>
    ///     Идентификатор
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Геопозиция (X,Y)
    /// </summary>
    public Location Location { get; set; }
}

public class Location
{
    /// <summary>
    ///     Горизонталь
    /// </summary>
    public int X { get; set; }

    /// <summary>
    ///     Вертикаль
    /// </summary>
    public int Y { get; set; }
}