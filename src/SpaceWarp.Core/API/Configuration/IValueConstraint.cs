using BepInEx.Configuration;

namespace SpaceWarp.API.Configuration;

/// <summary>
/// A constraint that can be applied to a <see cref="ConfigEntry{T}"/> to limit the values it can take.
/// </summary>
public interface IValueConstraint
{
    /// <summary>
    /// Checks if the given object is constrained by this constraint.
    /// </summary>
    /// <param name="o">Object to check.</param>
    /// <returns>True if the object is constrained, false otherwise.</returns>
    public bool IsConstrained(object o);

    /// <summary>
    /// Converts this constraint to an <see cref="AcceptableValueBase"/>.
    /// </summary>
    /// <returns>An <see cref="AcceptableValueBase"/> representing this constraint.</returns>
    public AcceptableValueBase ToAcceptableValueBase();
}