using CSharpFunctionalExtensions;
using Dapper;
using DeliveryApp.Core.Application.UseCases.Queries.GetCreatedAndAssignedOrders;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.Queries.GetCreatedAndAssignedOrders;

public class GetCreatedAndAssignedOrdersHandler 
    : IRequestHandler<GetCreatedAndAssignedOrdersCommand, Maybe<GetCreatedAndAssignedOrdersResponse>>
{
    private readonly string _connectionString;

    public GetCreatedAndAssignedOrdersHandler(string connectionString)
    {
        _connectionString = !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<Maybe<GetCreatedAndAssignedOrdersResponse>> 
        Handle(GetCreatedAndAssignedOrdersCommand request, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        
        var result = await connection.QueryAsync<dynamic>(
            @"SELECT
                o.id as id,
                o.location_x as location_x,
                o.location_y as location_y
                FROM public.orders as o
                WHERE o.status in ('created', 'assigned');");

        if (result.AsList().Count == 0)
            return null;
        
        return new GetCreatedAndAssignedOrdersResponse(MapOrder(result));
    }

    public List<Order> MapOrder(dynamic result)
    {
        var couriers = new List<Order>();
        foreach (var dItem in result)
        {
            var item = new Order() { Id = dItem.id, Location = new Location() 
                { X = dItem.location_x, Y = dItem.location_y } };
            couriers.Add(item);
        }
        return couriers;
    }
}