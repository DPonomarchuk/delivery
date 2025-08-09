using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;

public class CourierRepository : ICourierRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CourierRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddAsync(Courier courier)
    {
        await _dbContext.Couriers.AddAsync(courier);
    }

    public void Update(Courier courier)
    {
        _dbContext.Couriers.Update(courier);
    }

    public async Task<Maybe<Courier>> GetAsync(Guid courierId)
    {
        var courier = await _dbContext
            .Couriers
            .Include(c => c.StoragePlaces)
            .SingleOrDefaultAsync(o => o.Id == courierId);
        return courier;
    }

    public async Task<List<Courier>> GetAllFree()
    {
        return await _dbContext.Couriers.Include(c => c.StoragePlaces)
            .Where(c => c.StoragePlaces.All(x => x.OrderId == null)).ToListAsync();
    }
}