using System.Collections.Generic;
using JetBrains.Annotations;

namespace SpaceWarp2.API.Features;

/// <summary>
/// A feature a mod declares, with its relations to other features.
/// </summary>
[PublicAPI]
public class FeatureDefinition
{
    /// <summary>
    /// Gets the qualified id, <c>ModId:FeatureId</c>.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the id of the mod that declared the feature.
    /// </summary>
    public string ModId { get; }

    /// <summary>
    /// Gets the qualified ids of the features this one turns on with it.
    /// </summary>
    public IReadOnlyCollection<string> Requires { get; }

    /// <summary>
    /// Gets the qualified ids of the features that cannot be on in the same save as this one.
    /// </summary>
    public IReadOnlyCollection<string> Conflicts { get; }

    /// <summary>
    /// Creates a feature definition.
    /// </summary>
    /// <param name="id">The qualified feature id.</param>
    /// <param name="modId">The id of the declaring mod.</param>
    /// <param name="requires">The qualified ids of the required features.</param>
    /// <param name="conflicts">The qualified ids of the conflicting features.</param>
    public FeatureDefinition(
        string id,
        string modId,
        IReadOnlyCollection<string> requires,
        IReadOnlyCollection<string> conflicts
    )
    {
        Id = id;
        ModId = modId;
        Requires = requires;
        Conflicts = conflicts;
    }
}
