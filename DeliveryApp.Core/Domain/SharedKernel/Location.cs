using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Primitives;

namespace DeliveryApp.Core.Domain.SharedKernel;

/// <summary>
/// Координата на доске.
/// </summary>
public class Location : ValueObject
{
    /// <summary>
    ///     Конструктор
    /// </summary>
    [ExcludeFromCodeCoverage]
    private Location() { }

    /// <summary>
    ///     Конструктор
    /// </summary>
    /// <param name="x">Горизонталь</param>
    /// <param name="y">Вертикаль</param>
    private Location(short x, short y) : this()
    {
        X = x;
        Y = y;
    }

    private const short MinCoordinateValue = 1;
    private const short MaxCoordinateValue = 10;

    /// <summary>
    /// Координата по горизонтали
    /// </summary>
    public short X { get; }

    /// <summary>
    /// Координата по вертикали
    /// </summary>
    public short Y { get; }

    /// <summary>
    /// Создание координаты по заданным параметрам
    /// </summary>
    /// <param name="x">Значение по горизонтали</param>
    /// <param name="y">Значение по вертикали</param>
    /// <returns></returns>
    public static Result<Location, Error> Create(short x, short y)
    {
        if (x < MinCoordinateValue) return GeneralErrors.ValueIsRequired(nameof(x));
        if (y < MinCoordinateValue) return GeneralErrors.ValueIsRequired(nameof(y));
        if (x > MaxCoordinateValue) return GeneralErrors.ValueIsInvalid(nameof(x));
        if (y > MaxCoordinateValue) return GeneralErrors.ValueIsInvalid(nameof(y));
        return new Location(x, y);
    }

    /// <summary>
    /// Создание координаты со случайными значениями
    /// </summary>
    /// <returns></returns>
    public static Location CreateRandom()
    {
        var random = new Random();
        return Create((short)random.Next(MinCoordinateValue, MaxCoordinateValue+1),
            (short)random.Next(MinCoordinateValue, MaxCoordinateValue+1)).Value;
    }

    /// <summary>
    /// Вычислить расстояние до другой координаты
    /// </summary>
    /// <param name="other">Координата</param>
    /// <returns>Расстояние</returns>
    public short CalculateDistance(Location other)
    {
        return (short)(Math.Abs(X - other.X) + Math.Abs(Y - other.Y));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
    }
}