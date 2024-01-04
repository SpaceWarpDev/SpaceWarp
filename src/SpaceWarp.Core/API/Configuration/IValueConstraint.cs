using System.Reflection;
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

    /// <summary>
    /// Converts an acceptable value base into an IValueConstraint
    /// </summary>
    /// <param name="acceptableValueBase">The acceptable value base</param>
    /// <returns>The IValueConstraint</returns>
    public static IValueConstraint FromAcceptableValueBase(AcceptableValueBase acceptableValueBase)
    {
        if (acceptableValueBase is null) return null;
        if (acceptableValueBase.GetType().GetGenericTypeDefinition() == typeof(AcceptableValueList<>))
        {
            var type = acceptableValueBase.GetType().GetGenericArguments()[0];
            var valuesMethod = acceptableValueBase.GetType()
                .GetProperty("AcceptableValues", BindingFlags.Instance | BindingFlags.Public)
                ?.GetMethod;
            var values = valuesMethod?.Invoke(acceptableValueBase,[]);
            return Activator.CreateInstance(typeof(ListConstraint<>).MakeGenericType(type), [values]) as IValueConstraint;
        }

        if (acceptableValueBase.GetType().GetGenericTypeDefinition() == typeof(AcceptableValueRange<>))
        {
            var type = acceptableValueBase.GetType().GetGenericArguments()[0];
            var minMethod = acceptableValueBase.GetType()
                .GetProperty("MinValue", BindingFlags.Instance | BindingFlags.Public)?.GetMethod;
            var maxMethod = acceptableValueBase.GetType()
                .GetProperty("MaxValue", BindingFlags.Instance | BindingFlags.Public)?.GetMethod;
            var min = minMethod?.Invoke(acceptableValueBase, []);
            var max = maxMethod?.Invoke(acceptableValueBase, []);
            return Activator.CreateInstance(typeof(RangeConstraint<>).MakeGenericType(type),[min,max]) as IValueConstraint;
        }

        return null;
    }
}