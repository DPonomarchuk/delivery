using Confluent.Kafka;
using DeliveryApp.Core.Application.Commands.CreateOrder;
using MediatR;
using Newtonsoft.Json;

namespace DeliveryApp.Api.Adapters.Kafka.BasketChanged;

public class ConsumerService : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IMediator _mediator;
    private readonly string _topic;

    public ConsumerService(IMediator mediator, string messageBrokerHost, string topic)
    {
        _mediator = mediator ??  throw new ArgumentNullException(nameof(mediator));
        _topic = topic ?? throw new ArgumentNullException(nameof(topic));
        if (string.IsNullOrEmpty(messageBrokerHost)) 
            throw new ArgumentNullException(nameof(messageBrokerHost));

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = messageBrokerHost,
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
            /* while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                var consumeResult = _consumer.Consume(stoppingToken);

                if (consumeResult.IsPartitionEOF) continue;

                var basketChangedIntegrationEvent =
                    JsonConvert.DeserializeObject<BasketConfirmedIntegrationEvent>(consumeResult.Message.Value);

                var createOrderCommandResult = CreateOrderCommand.Create(
                    Guid.Parse(basketChangedIntegrationEvent.BasketId),
                    basketChangedIntegrationEvent.Quantity);
                if (createOrderCommandResult.IsFailure) Console.WriteLine(createOrderCommandResult.Error);

                var sendResult = await _mediator.Send(createOrderCommandResult.Value, stoppingToken);
                if (sendResult.IsFailure) Console.WriteLine(sendResult.Error);

                try
                {
                    _consumer.StoreOffset(consumeResult);
                }
                catch (KafkaException e)
                {
                    Console.WriteLine($"Store Offset error: {e.Error.Reason}");
                }
            }*/
        }
        catch (OperationCanceledException e)
        {
            Console.WriteLine(e.Message);
        }
    }
}