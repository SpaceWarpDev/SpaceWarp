using System;
using JetBrains.Annotations;

namespace SpaceWarp2.API.Features;

/// <summary>
/// Marks a bool field of a mod's plugin as following a feature: true while the active set has the feature on.
/// </summary>
/// <remarks>
/// The field is written whenever a set activates, which is on boot, on loading a campaign and on returning to the
/// main menu. A bare id belongs to the plugin's own mod.
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Field)]
public class FeatureAttribute : Attribute
{
    /// <summary>
    /// Marks the field as following a feature.
    /// </summary>
    /// <param name="featureId">The feature id, bare or qualified.</param>
    public FeatureAttribute(string featureId)
    {
        FeatureId = featureId;
    }

    /// <summary>
    /// Gets the feature id as written, bare or qualified.
    /// </summary>
    public string FeatureId { get; }
}
