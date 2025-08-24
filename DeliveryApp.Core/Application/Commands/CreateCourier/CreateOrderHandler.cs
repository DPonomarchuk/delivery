using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Commands.CreateCourier;

public class CreateCourierHandler : IRequestHandler<CreateCourierCommand, UnitResult<Error>>
{
    private readonly ICourierRepository _courierRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    /// <summary>
    ///     Ctr
    /// </summary>
    public CreateCourierHandler(IUnitOfWork unitOfWork, ICourierRepository courierRepository)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _courierRepository = courierRepository ?? throw new ArgumentNullException(nameof(courierRepository));
    }
    
    public async Task<UnitResult<Error>> Handle(CreateCourierCommand request, CancellationToken cancellationToken)
    {
        // Создаем случайную позицию, т.к. сервис не передаёт.
        var courierLocation = Location.CreateRandom();
        
        var courierCreateResult = Courier.Create(request.Name, request.Speed, courierLocation);
        if (courierCreateResult.IsFailure) return courierCreateResult;
        var courier = courierCreateResult.Value;

        await _courierRepository.AddAsync(courier);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }
}