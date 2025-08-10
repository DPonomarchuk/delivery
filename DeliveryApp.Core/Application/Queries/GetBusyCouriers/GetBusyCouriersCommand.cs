using CSharpFunctionalExtensions;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Queries.GetBusyCouriers;

public class GetBusyCouriersCommand : IRequest<Maybe<GetCouriersResponse>>
{
}