using DeliveryApp.Infrastructure.Adapters.Postgres.Entities;
using MediatR;
using Newtonsoft.Json;
using Primitives;

namespace DeliveryApp.Infrastructure.Adapters.Postgres;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMediator _mediator;
    private bool _disposed;

    public UnitOfWork(ApplicationDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mediator = mediator;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await SaveDomainEventsInOutboxMessagesAsync();
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing) _dbContext.Dispose();
            _disposed = true;
        }
    }

    private async Task SaveDomainEventsInOutboxMessagesAsync()
    {
        var outboxMessages = _dbContext.ChangeTracker
            .Entries<IAggregateRoot>() // Получили агрегаты в которых есть доменные события
            .Select(x => x.Entity)
            .SelectMany(aggregate =>
                {
                    // Переложили в отдельную переменную
                    var domainEvents = aggregate.GetDomainEvents();

                    // Очистили Domain Event в самих агрегатах (поскольку далее они будут отправлены и больше не нужны)
                    aggregate.ClearDomainEvents();
                    return domainEvents;
                }
            )
            .Select(domainEvent => new OutboxMessage
            {
                // Создали объект OutboxMessage на основе Domain Event
                Id = domainEvent.EventId,
                OccurredOnUtc = DateTime.UtcNow,
                Type = domainEvent.GetType().Name,
                Content = JsonConvert.SerializeObject(
                    domainEvent,
                    new JsonSerializerSettings
                    {
                        // Эта настройка нужна, чтобы сериализовать Domain Event с указанием типов
                        // Если ее не указать, то десеарилизатор не поймет в какой тип восстанавоивать сообщение
                        TypeNameHandling = TypeNameHandling.All
                    })
            })
            .ToList();

        // Добавляем OutboxMessages в dbContext
        // После выполнения этой строки в DbContext будут находиться сам Aggregate и OutboxMessages
        await _dbContext.Set<OutboxMessage>().AddRangeAsync(outboxMessages);
    }
}