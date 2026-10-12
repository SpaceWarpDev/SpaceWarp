using System;
using JetBrains.Annotations;
using ReduxLib.Configuration.Attributes;

namespace SpaceWarp2.API.Features;

/// <summary>
/// Shows a config value in the settings menu only while a feature is off, alongside
/// <see cref="ConfigValueAttribute" />.
/// </summary>
/// <remarks>
/// A bare id belongs to the mod whose config holds the value.
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public class ConfigNoFeatureAttribute : ConfigMetadataAttribute
{
    /// <summary>
    /// Shows the value only while the feature is off.
    /// </summary>
    /// <param name="featureId">The feature id, bare or qualified.</param>
    public ConfigNoFeatureAttribute(string featureId) : base(ActiveFeatures.NO_FEATURE_TAG_PREFIX + featureId)
    {
    }
}
