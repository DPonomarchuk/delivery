using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _dbContext;

    public OrderRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(Order order)
    {
        await _dbContext.Orders.AddAsync(order);
    }

    public void Update(Order order)
    {
        _dbContext.Orders.Update(order);
    }

    public async Task<Maybe<Order>> GetAsync(Guid orderId)
    {
        var order = await _dbContext
            .Orders
            .SingleOrDefaultAsync(o => o.Id == orderId);
        return order;
    }

    public async Task<Maybe<Order>> GetFirstInCreatedStatusAsync()
    {
        var order = await _dbContext
            .Orders
            .FirstOrDefaultAsync(o => o.Status.Name == OrderStatus.Created.Name);
        return order;
    }

    public IEnumerable<Order> GetAllInAssignedStatus()
    {
        var orders = _dbContext
            .Orders
            .Where(o => o.Status.Name == OrderStatus.Assigned.Name);
        return orders;
    }
}