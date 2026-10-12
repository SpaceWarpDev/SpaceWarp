using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    private const BindingFlags FIELD_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    // Every set activation binds every plugin, so each type is scanned once
    private static readonly Dictionary<Type, (FieldInfo field, string featureId)[]> FeatureFieldsByType = new();

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
                BindFeatureFields(descriptor.Plugin, descriptor.Guid);
            }
        }

        ModScriptRuntime.RefreshFeatureEnvironments();
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
    public static bool IsModEnabled(string modId) => IsEnabled(FeatureRegistry.DefaultFor(modId));

    /// <summary>
    /// Checks whether a mod's runtime hooks run in the active feature set.
    /// </summary>
    /// <remarks>
    /// The console is not a mod, so its hooks always run.
    /// </remarks>
    /// <param name="modId">The id the hooks were registered under.</param>
    /// <returns>True if the hooks run.</returns>
    public static bool RunsHooksOf(string modId) => modId == ModScriptRuntime.ConsoleModId || IsModEnabled(modId);

    /// <summary>
    /// The tag prefix of a config entry shown only while a feature is on, such as <c>feature:Fast</c>.
    /// </summary>
    public const string FEATURE_TAG_PREFIX = "feature:";

    /// <summary>
    /// The tag prefix of a config entry shown only while a feature is off.
    /// </summary>
    public const string NO_FEATURE_TAG_PREFIX = "nofeature:";

    /// <summary>
    /// Checks whether the active set shows a config entry with the given tags.
    /// </summary>
    /// <param name="modId">The mod the entry belongs to, which bare feature ids in its tags belong to.</param>
    /// <param name="tags">The entry's tags.</param>
    /// <returns>True if every feature the tags select is on and every one they exclude is off.</returns>
    public static bool AllowsConfigEntry(string modId, IEnumerable<string> tags) => true;

    /// <summary>
    /// Sets each <see cref="FeatureAttribute" /> bool field of an object to whether its feature is on.
    /// </summary>
    /// <param name="target">The object, normally a mod's plugin.</param>
    /// <param name="modId">The mod bare feature ids belong to.</param>
    public static void BindFeatureFields(object target, string modId)
    {
        foreach (var (field, featureId) in FeatureFieldsOf(target.GetType()))
        {
            field.SetValue(target, IsEnabled(FeatureRegistry.Qualify(modId, featureId)));
        }
    }

    private static (FieldInfo field, string featureId)[] FeatureFieldsOf(Type type)
    {
        if (FeatureFieldsByType.TryGetValue(type, out var fields))
            return fields;

        fields = HierarchyOf(type)
            .SelectMany(level => level.GetFields(FIELD_FLAGS | BindingFlags.DeclaredOnly))
            .Where(field => field.FieldType == typeof(bool))
            .Select(field => (field, attribute: field.GetCustomAttribute<FeatureAttribute>()))
            .Where(pair => pair.attribute != null)
            .Select(pair => (pair.field, pair.attribute!.FeatureId))
            .ToArray();
        FeatureFieldsByType[type] = fields;
        return fields;
    }

    // Private fields of base classes are only reachable from the class that declares them
    private static IEnumerable<Type> HierarchyOf(Type type)
    {
        for (Type? level = type; level != null && level != typeof(object); level = level.BaseType)
        {
            yield return level;
        }
    }
}
