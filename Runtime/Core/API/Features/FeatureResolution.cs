using System.Collections.Generic;
using JetBrains.Annotations;

namespace SpaceWarp2.API.Features;

/// <summary>
/// The outcome of resolving a requested set of features against a <see cref="FeatureRegistry" />.
/// </summary>
[PublicAPI]
public class FeatureResolution
{
    /// <summary>
    /// Gets the known features that end up on: the request plus everything it requires, in ordinal order.
    /// </summary>
    public IReadOnlyList<string> Enabled { get; }

    /// <summary>
    /// Gets the requested or required ids that no installed mod declares, in ordinal order.
    /// </summary>
    public IReadOnlyList<string> Missing { get; }

    /// <summary>
    /// Gets each pair of enabled features where one conflicts with the other.
    /// </summary>
    public IReadOnlyList<FeatureConflict> Conflicts { get; }

    /// <summary>
    /// Creates a resolution.
    /// </summary>
    /// <param name="enabled">The enabled feature ids, in ordinal order.</param>
    /// <param name="missing">The unknown feature ids, in ordinal order.</param>
    /// <param name="conflicts">The conflicting pairs.</param>
    public FeatureResolution(
        IReadOnlyList<string> enabled,
        IReadOnlyList<string> missing,
        IReadOnlyList<FeatureConflict> conflicts
    )
    {
        Enabled = enabled;
        Missing = missing;
        Conflicts = conflicts;
    }
}

/// <summary>
/// Two enabled features where at least one declares a conflict with the other.
/// </summary>
[PublicAPI]
public class FeatureConflict
{
    /// <summary>
    /// Gets the id that sorts first ordinally.
    /// </summary>
    public string First { get; }

    /// <summary>
    /// Gets the id that sorts second ordinally.
    /// </summary>
    public string Second { get; }

    /// <summary>
    /// Creates a conflict between two features, in either order.
    /// </summary>
    /// <param name="a">One feature id.</param>
    /// <param name="b">The other feature id.</param>
    public FeatureConflict(string a, string b)
    {
        bool aFirst = string.CompareOrdinal(a, b) <= 0;
        First = aFirst ? a : b;
        Second = aFirst ? b : a;
    }
}
