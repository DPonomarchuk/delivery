using CSharpFunctionalExtensions;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Commands.MoveCouriers;

public class MoveCouriersCommand : IRequest<UnitResult<Error>>
{
}