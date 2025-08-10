using CSharpFunctionalExtensions;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Commands.AssignOrder;

/// <summary>
/// Назначить заказ на курьера
/// </summary>
public class AssignOrderCommand : IRequest<UnitResult<Error>>
{
}