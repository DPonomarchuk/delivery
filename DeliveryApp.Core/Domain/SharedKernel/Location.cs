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
        if (x <= 0) return GeneralErrors.ValueIsRequired(nameof(x));
        if (y <= 0) return GeneralErrors.ValueIsRequired(nameof(y));
        if (x > 10) return GeneralErrors.ValueIsInvalid(nameof(x));
        if (y > 10) return GeneralErrors.ValueIsInvalid(nameof(y));
        return new Location(x, y);
    }

    /// <summary>
    /// Создание координаты со случайными значениями
    /// </summary>
    /// <returns></returns>
    public static Result<Location, Error> CreateRandom()
    {
        var random = new Random();
        return Create((short)random.Next(1, 10), (short)random.Next(1, 10));
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

    protected bool Equals(Location other)
    {
        return base.Equals(other) && X == other.X && Y == other.Y;
    }

    public static bool operator ==(Location left, Location right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Location left, Location right)
    {
        return !(left == right);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
    }
}