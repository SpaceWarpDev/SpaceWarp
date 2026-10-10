using JetBrains.Annotations;

namespace SpaceWarp2.API.Features;

/// <summary>
/// Builds and splits namespaced feature ids of the form <c>ModId:FeatureId</c>.
/// </summary>
[PublicAPI]
public static class FeatureIds
{
    /// <summary>
    /// The character between the mod id and the feature id.
    /// </summary>
    public const char SEPARATOR = ':';

    /// <summary>
    /// Qualifies a feature id with a mod id unless it already names a mod.
    /// </summary>
    /// <param name="modId">The mod the id is relative to.</param>
    /// <param name="featureId">A bare or already qualified feature id.</param>
    /// <returns>The qualified feature id.</returns>
    public static string Qualify(string modId, string featureId) =>
        featureId.IndexOf(SEPARATOR) >= 0 ? featureId : $"{modId}{SEPARATOR}{featureId}";

    /// <summary>
    /// Gets the id of a mod's default feature.
    /// </summary>
    /// <param name="modId">The mod id.</param>
    /// <returns>The default feature id, <c>ModId:ModId</c>.</returns>
    public static string DefaultFor(string modId) => $"{modId}{SEPARATOR}{modId}";

    /// <summary>
    /// Gets the namespace of a qualified feature id, the part before the separator.
    /// </summary>
    /// <param name="featureId">The qualified feature id.</param>
    /// <returns>The namespace, or the whole id when it has no separator.</returns>
    public static string NamespaceOf(string featureId)
    {
        int separator = featureId.IndexOf(SEPARATOR);
        return separator < 0 ? featureId : featureId.Substring(0, separator);
    }
}
