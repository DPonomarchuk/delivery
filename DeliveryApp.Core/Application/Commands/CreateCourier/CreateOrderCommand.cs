using CSharpFunctionalExtensions;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Commands.CreateCourier;

public class CreateCourierCommand : IRequest<UnitResult<Error>>
{
    private CreateCourierCommand(string name, int speed)
    {
        Name = name;
        Speed = speed;
    }
    
    /// <summary>
    ///     Factory Method
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="speed">Скорость</param>
    /// <returns>Результат</returns>
    public static Result<CreateCourierCommand, Error> Create(string name, int speed)
    {
        if (string.IsNullOrWhiteSpace(name)) return GeneralErrors.ValueIsRequired(nameof(name));
        if (speed <= 0) return GeneralErrors.ValueIsInvalid(nameof(speed));

        return new CreateCourierCommand(name, speed);
    }

    /// <summary>
    ///     Имя
    /// </summary>
    /// <remarks>Имя курьера</remarks>
    public string Name { get; }
    
    /// <summary>
    ///     Скорость
    /// </summary>
    public int Speed { get; }
}