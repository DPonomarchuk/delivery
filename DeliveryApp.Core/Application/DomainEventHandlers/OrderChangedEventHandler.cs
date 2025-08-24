using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using MediatR;

namespace DeliveryApp.Core.Application.DomainEventHandlers;

public class OrderChangedEventHandler : INotificationHandler<OrderChangedDomainEvent>
{
    private readonly IMessageBusProducer _messageBusProducer;

    public OrderChangedEventHandler(IMessageBusProducer messageBusProducer)
    {
        _messageBusProducer = messageBusProducer;
    }

    public async Task Handle(OrderChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        await _messageBusProducer.Publish(notification, cancellationToken);
    }
}