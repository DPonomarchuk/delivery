using Confluent.Kafka;
using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using Newtonsoft.Json;
using OrderStatusChanged;

namespace DeliveryApp.Infrastructure.Adapters.Kafka.OrderStatusChanged;

public class Producer : IMessageBusProducer
{
    private readonly ProducerConfig _producerConfig;
    private readonly string _topicName = "order.status.changed";
    
    public Producer(string messageBrokerHost)
    {
        if (string.IsNullOrEmpty(messageBrokerHost)) throw new ArgumentNullException(nameof(messageBrokerHost));
        _producerConfig = new ProducerConfig()
        {
            BootstrapServers = messageBrokerHost,
        };
    }
    public async Task Publish(OrderChangedDomainEvent orderChangedDomainEvent, CancellationToken cancellationToken)
    {
        var orderChangedIntegrationEvent = new OrderStatusChangedIntegrationEvent()
        {
            OrderId = orderChangedDomainEvent.Order.Id.ToString(),
            OrderStatus = GetOrderStatus(orderChangedDomainEvent.Order.Status.Name)
        };
        var message = new Message<string, string>()
        {
            Key = orderChangedDomainEvent.Order.Id.ToString(),
            Value = JsonConvert.SerializeObject(orderChangedIntegrationEvent)
        };

        try
        {
            using var producerClient = new ProducerBuilder<string, string>(_producerConfig).Build();
            var result = await producerClient.ProduceAsync(_topicName, message, cancellationToken);
            Console.WriteLine($"Delivered order {result.Value} to {result.TopicPartitionOffset}");
        }
        catch (ProduceException<Null, string> e)
        {
            Console.WriteLine($"Delivery failed: {e.Error.Reason}");
        }
    }

    private OrderStatus GetOrderStatus(string status)
    {
        return status switch
        {
            "completed" => OrderStatus.Completed,
            "assigned" => OrderStatus.Assigned,
            "created" => OrderStatus.Created,
            _ => OrderStatus.None
        };
    }
}