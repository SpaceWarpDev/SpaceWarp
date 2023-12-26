using BepInEx.Configuration;
using JetBrains.Annotations;

namespace SpaceWarp.API.Configuration;

/// <summary>
/// A constraint that checks if a value is within a range.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class RangeConstraint<T> : ValueConstraint<T> where T : IComparable<T>, IComparable
{
    /// <summary>
    /// The minimum value.
    /// </summary>
    [PublicAPI] public T Minimum;

    /// <summary>
    /// The maximum value.
    /// </summary>
    [PublicAPI] public T Maximum;

    /// <summary>
    /// Creates a new range constraint.
    /// </summary>
    /// <param name="minimum">The minimum value.</param>
    /// <param name="maximum">The maximum value.</param>
    public RangeConstraint(T minimum, T maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    /// <inheritdoc />
    public override bool IsValid(T o) => Minimum.CompareTo(o) <= 0 && Maximum.CompareTo(o) >= 0;

    /// <inheritdoc />
    public override AcceptableValueBase ToAcceptableValueBase()
    {
        return new AcceptableValueRange<T>(Minimum, Maximum);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Minimum} - {Maximum}";
    }
}