using Primitives;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;

public sealed record OrderChangedDomainEvent(Order Order) : DomainEvent();