using CSharpFunctionalExtensions;
using Dapper;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.Queries.GetBusyCouriers;

public class GetBusyCouriersHandler : IRequestHandler<GetBusyCouriersCommand, Maybe<GetCouriersResponse>>
{
    private readonly string _connectionString;

    public GetBusyCouriersHandler(string connectionString)
    {
        _connectionString = !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : throw new ArgumentNullException(nameof(connectionString));

    }
    
    public async Task<Maybe<GetCouriersResponse>> 
        Handle(GetBusyCouriersCommand request, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        
        var result = await connection.QueryAsync<dynamic>(
            @"SELECT
                c.id as id,
                c.name as name,
                c.location_x as location_x,
                c.location_y as location_y
                FROM public.couriers as c
                JOIN public.orders as o on o.courier_id = c.id;");

        if (result.AsList().Count == 0)
            return null;

        return new GetCouriersResponse(MapCourier(result));
    }
    
    private List<Courier> MapCourier(dynamic result)
    {
        var couriers = new List<Courier>();
        foreach (var dItem in result)
        {
            var item = new Courier() { Id = dItem.id, Name = dItem.name, 
                Location = new Location() { X = dItem.location_x, Y = dItem.location_y } };
            couriers.Add(item);
        }
        return couriers;
    }
}