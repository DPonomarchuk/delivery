using CSharpFunctionalExtensions;
using DeliveryApp.Core.Application.UseCases.Queries.GetCreatedAndAssignedOrders;
using MediatR;

namespace DeliveryApp.Core.Application.Queries.GetCreatedAndAssignedOrders;

public class GetCreatedAndAssignedOrdersCommand : IRequest<Maybe<GetCreatedAndAssignedOrdersResponse>>
{
    
}