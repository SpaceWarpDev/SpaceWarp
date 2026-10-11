using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SpaceWarp2.API.Lifecycle;
using SpaceWarp2.API.Mods;

namespace SpaceWarp2.API.Features;

/// <summary>
/// The features the game currently runs with: the main menu's set at the menu, and the loaded campaign's set
/// in a campaign.
/// </summary>
/// <remarks>
/// Before any set activates, every feature reads as enabled, since the main menu's set that boot activates turns
/// on every installed mod.
/// </remarks>
[PublicAPI]
public static class ActiveFeatures
{
    private static HashSet<string>? _enabled;

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        _enabled = null;
    }

    /// <summary>
    /// Gets the enabled feature ids, or null before any set has activated.
    /// </summary>
    public static IReadOnlyCollection<string>? Enabled => _enabled;

    /// <summary>
    /// Makes a set the active one, and updates every plugin's <see cref="FeatureAttribute" /> fields and every mod
    /// script environment's <c>Enabled</c> to match.
    /// </summary>
    /// <param name="features">The set's enabled features, or null for no set.</param>
    public static void Activate(IEnumerable<string>? features)
    {
        _enabled = features == null ? null : new HashSet<string>(features, StringComparer.OrdinalIgnoreCase);
        foreach (SpaceWarpPluginDescriptor descriptor in PluginList.AllEnabledAndActivePlugins)
        {
            if (descriptor.Plugin != null)
            {
                FeatureFields.Bind(descriptor.Plugin, descriptor.Guid);
            }
        }

        FeatureEnvironments.Refresh();
    }

    /// <summary>
    /// Checks whether a feature is on in the active set.
    /// </summary>
    /// <param name="featureId">The qualified feature id.</param>
    /// <returns>True if the feature is on, or if no set is active.</returns>
    public static bool IsEnabled(string featureId) => _enabled == null || _enabled.Contains(featureId);

    /// <summary>
    /// Checks whether a mod's default feature is on in the active set.
    /// </summary>
    /// <param name="modId">The mod id.</param>
    /// <returns>True if the mod is on, or if no set is active.</returns>
    public static bool IsModEnabled(string modId) => IsEnabled(FeatureIds.DefaultFor(modId));
}
