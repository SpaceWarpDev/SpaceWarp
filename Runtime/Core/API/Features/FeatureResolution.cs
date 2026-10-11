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
    /// Gets each pair of enabled features where one conflicts with the other, the ordinally first id first.
    /// </summary>
    public IReadOnlyList<(string First, string Second)> Conflicts { get; }

    /// <summary>
    /// Creates a resolution.
    /// </summary>
    /// <param name="enabled">The enabled feature ids, in ordinal order.</param>
    /// <param name="missing">The unknown feature ids, in ordinal order.</param>
    /// <param name="conflicts">The conflicting pairs.</param>
    public FeatureResolution(
        IReadOnlyList<string> enabled,
        IReadOnlyList<string> missing,
        IReadOnlyList<(string First, string Second)> conflicts
    )
    {
        Enabled = enabled;
        Missing = missing;
        Conflicts = conflicts;
    }
}
