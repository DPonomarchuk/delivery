using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Primitives;
using JsonNet.ContractResolvers;
using Quartz;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.BackgroundJobs;

public class ProcessOutboxMessagesJob : IJob
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMediator _mediator;

    public ProcessOutboxMessagesJob(ApplicationDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        var outboxMessages = await _dbContext.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(100)
            .ToListAsync();

        if (outboxMessages.Any())
        {
            foreach (var outboxMessage in outboxMessages)
            {
                var settings = new JsonSerializerSettings()
                {
                    ContractResolver = new PrivateSetterContractResolver(),
                    TypeNameHandling = TypeNameHandling.All
                };
                
                var domainEvent = JsonConvert.DeserializeObject<DomainEvent>(outboxMessage.Content, settings);
                await _mediator.Publish(domainEvent);
                outboxMessage.ProcessedOnUtc = DateTime.UtcNow;
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}