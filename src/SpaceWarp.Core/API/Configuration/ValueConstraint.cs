using BepInEx.Configuration;

namespace SpaceWarp.API.Configuration;

/// <summary>
/// Base class for value constraints.
/// </summary>
/// <typeparam name="T">Type of the value.</typeparam>
public abstract class ValueConstraint<T> : IValueConstraint
{
    /// <summary>
    /// Returns true if the given value is valid for this constraint.
    /// </summary>
    /// <param name="o">Value to check.</param>
    /// <returns>True if the value is valid, false otherwise.</returns>
    public abstract bool IsValid(T o);

    /// <summary>
    /// Returns true if the given value is valid for this constraint.
    /// </summary>
    /// <param name="o">Value to check.</param>
    /// <returns>True if the value is valid, false otherwise.</returns>
    public bool IsValid(object o)
    {
        return IsValid((T)o);
    }

    /// <inheritdoc />
    public bool IsConstrained(object o)
    {
        return IsValid((T)o);
    }

    /// <inheritdoc />
    public abstract AcceptableValueBase ToAcceptableValueBase();
}