using System.Text;
using BepInEx.Configuration;
using JetBrains.Annotations;

namespace SpaceWarp.API.Configuration;

/// <summary>
/// A constraint that checks if the value is in a list of acceptable values.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class ListConstraint<T> : ValueConstraint<T> where T : IEquatable<T>
{
    /// <summary>
    /// The list of acceptable values.
    /// </summary>
    [PublicAPI] public List<T> AcceptableValues;

    /// <summary>
    /// Creates a new list constraint.
    /// </summary>
    /// <param name="acceptableValues">The list of acceptable values.</param>
    public ListConstraint(IEnumerable<T> acceptableValues)
    {
        AcceptableValues = acceptableValues.ToList();
    }

    /// <summary>
    /// Creates a new list constraint.
    /// </summary>
    /// <param name="acceptableValues">The array of acceptable values.</param>
    public ListConstraint(params T[] acceptableValues)
    {
        AcceptableValues = acceptableValues.ToList();
    }

    /// <inheritdoc />
    public override bool IsValid(T o) => AcceptableValues.Any(x => x.Equals(o));

    /// <inheritdoc />
    public override AcceptableValueBase ToAcceptableValueBase()
    {
        return new AcceptableValueList<T>(AcceptableValues.ToArray());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("[");
        for (int i = 0; i < AcceptableValues.Count; i++)
        {
            if (i != 0)
            {
                sb.Append(", ");
            }

            sb.Append(AcceptableValues[i]);
        }

        sb.Append("]");
        return sb.ToString();
    }
}