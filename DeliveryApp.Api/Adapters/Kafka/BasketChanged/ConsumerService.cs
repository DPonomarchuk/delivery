using BasketConfirmed;
using Confluent.Kafka;
using DeliveryApp.Core.Application.Commands.CreateOrder;
using DeliveryApp.Infrastructure;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DeliveryApp.Api.Adapters.Kafka.BasketChanged;

public class ConsumerService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly string _topic;

    public ConsumerService(IServiceScopeFactory serviceScopeFactory, IOptions<Settings> optionSettings)
    {
        ArgumentNullException.ThrowIfNull(optionSettings);
        _serviceScopeFactory = serviceScopeFactory;
        var settings = optionSettings.Value;
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _topic = settings.BasketConfirmedTopic ?? 
                 throw new ArgumentNullException(nameof(settings.BasketConfirmedTopic));
        if (string.IsNullOrEmpty(settings.MessageBrokerHost)) 
            throw new ArgumentNullException(nameof(settings.MessageBrokerHost));

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = settings.MessageBrokerHost,
            GroupId = "OrderConsumerGroup",
            EnableAutoOffsetStore = false,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            EnablePartitionEof = true
        };
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                var consumeResult = _consumer.Consume(stoppingToken);

                if (consumeResult.IsPartitionEOF) continue;

                var basketChangedIntegrationEvent =
                    JsonConvert.DeserializeObject<BasketConfirmedIntegrationEvent>(consumeResult.Message.Value);

                var createOrderCommandResult = CreateOrderCommand.Create(
                    Guid.Parse(basketChangedIntegrationEvent.BasketId),
                    basketChangedIntegrationEvent.Address.Street,
                    basketChangedIntegrationEvent.Volume);
                if (createOrderCommandResult.IsFailure) Console.WriteLine(createOrderCommandResult.Error);

                using var scope = _serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var sendResult = await mediator.Send(createOrderCommandResult.Value, stoppingToken);
                if (sendResult.IsFailure) Console.WriteLine(sendResult.Error);

                try
                {
                    _consumer.StoreOffset(consumeResult);
                }
                catch (KafkaException e)
                {
                    Console.WriteLine($"Store Offset error: {e.Error.Reason}");
                }
            }
        }
        catch (OperationCanceledException e)
        {
            Console.WriteLine(e.Message);
        }
    }
}