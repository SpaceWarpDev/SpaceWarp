using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SpaceWarp2.API.Mods.JSON;
using UnityEngine;

namespace SpaceWarp2.API.Features;

/// <summary>
/// Every feature the installed mods declare, and the rules that turn a requested set into an enabled one.
/// </summary>
/// <remarks>
/// Each mod gets a default feature, <c>ModId:ModId</c>, which requires the default features of its dependencies and
/// conflicts with those of its conflicts. Every other feature of a mod requires that mod's default feature. Ids
/// match case-insensitively and resolve to the casing they were declared with.
/// </remarks>
[PublicAPI]
public class FeatureRegistry
{
    private readonly Dictionary<string, FeatureDefinition> _features = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the registry built from the installed, enabled mods once their load order is resolved.
    /// </summary>
    public static FeatureRegistry Instance { get; private set; } = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState() => Instance = new FeatureRegistry();

    internal static void BuildInstance(IEnumerable<ModInfo> mods)
    {
        var registry = new FeatureRegistry();
        foreach (ModInfo mod in mods)
        {
            if (mod == null)
                continue;

            try
            {
                registry.AddMod(mod);
            }
            catch (Exception exception)
            {
                SpaceWarpPlugin.Logger.LogError($"Could not read the features of {mod.Name}: {exception.Message}");
            }
        }

        Instance = registry;
    }

    /// <summary>
    /// Gets every declared feature.
    /// </summary>
    public IEnumerable<FeatureDefinition> All => _features.Values;

    /// <summary>
    /// Declares a mod's default feature and the features in its mod info.
    /// </summary>
    /// <param name="info">The mod info.</param>
    public void AddMod(ModInfo info)
    {
        string modId = info.ModID;
        Define(
            modId,
            modId,
            info.Dependencies.Select(dependency => FeatureIds.DefaultFor(dependency.ID)),
            info.Conflicts.Select(conflict => FeatureIds.DefaultFor(conflict.ID))
        );

        foreach (KeyValuePair<string, FeatureInfo> feature in info.Features)
        {
            Define(modId, feature.Key, feature.Value.Requires, feature.Value.Conflicts);
        }
    }

    /// <summary>
    /// Declares one feature of a mod outside its mod info.
    /// </summary>
    /// <remarks>
    /// Declaring an id that already exists replaces the earlier declaration.
    /// </remarks>
    /// <param name="modId">The declaring mod.</param>
    /// <param name="featureId">The feature id, bare or qualified.</param>
    /// <param name="requires">The required feature ids, bare ones relative to the feature's namespace.</param>
    /// <param name="conflicts">The conflicting feature ids, bare ones relative to the feature's namespace.</param>
    /// <returns>The declared feature.</returns>
    public FeatureDefinition Define(
        string modId,
        string featureId,
        IEnumerable<string>? requires = null,
        IEnumerable<string>? conflicts = null
    )
    {
        string id = FeatureIds.Qualify(modId, featureId);

        // A feature belongs to the namespace in its id, which differs from the declaring mod when, for example,
        // Redux declares the Stock features. Bare relations resolve in that namespace too.
        string owner = FeatureIds.NamespaceOf(id);
        string defaultId = FeatureIds.DefaultFor(owner);

        // A namespace's own default feature is the one feature that does not require itself.
        IEnumerable<string> implicitRequires = string.Equals(id, defaultId, StringComparison.OrdinalIgnoreCase)
            ? Array.Empty<string>()
            : new[] { defaultId };

        var definition = new FeatureDefinition(
            id,
            owner,
            QualifyAll(owner, implicitRequires.Concat(requires ?? Array.Empty<string>())),
            QualifyAll(owner, conflicts ?? Array.Empty<string>())
        );
        _features[id] = definition;
        return definition;
    }

    /// <summary>
    /// Looks a feature up by id.
    /// </summary>
    /// <param name="featureId">The qualified feature id, in any casing.</param>
    /// <param name="definition">The feature, when found.</param>
    /// <returns>True if the feature is declared, false otherwise.</returns>
    public bool TryGet(string featureId, out FeatureDefinition definition) =>
        _features.TryGetValue(featureId, out definition);

    /// <summary>
    /// Checks whether a feature and everything it requires, transitively, is declared.
    /// </summary>
    /// <remarks>
    /// An unsatisfiable feature is hidden from players, for example one whose mod depends on a mod that is not
    /// installed.
    /// </remarks>
    /// <param name="featureId">The qualified feature id.</param>
    /// <returns>True if the feature can be turned on, false otherwise.</returns>
    public bool IsSatisfiable(string featureId) =>
        IsSatisfiable(featureId, new HashSet<string>(StringComparer.OrdinalIgnoreCase));

    /// <summary>
    /// Turns a requested set of features into the set that ends up on.
    /// </summary>
    /// <param name="requested">The requested qualified feature ids.</param>
    /// <returns>The enabled closure, the unknown ids, and the conflicts between enabled features.</returns>
    public FeatureResolution Resolve(IEnumerable<string> requested)
    {
        var enabled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var missing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new Stack<string>(requested);
        while (pending.Count > 0)
        {
            string id = pending.Pop();
            if (!_features.TryGetValue(id, out FeatureDefinition definition))
            {
                missing.Add(id);
                continue;
            }

            if (!enabled.Add(definition.Id))
                continue;

            foreach (string required in definition.Requires)
            {
                pending.Push(required);
            }
        }

        return new FeatureResolution(
            enabled.OrderBy(id => id, StringComparer.Ordinal).ToList(),
            missing.OrderBy(id => id, StringComparer.Ordinal).ToList(),
            FindConflicts(enabled)
        );
    }

    private bool IsSatisfiable(string featureId, HashSet<string> visited)
    {
        if (!_features.TryGetValue(featureId, out FeatureDefinition definition))
            return false;

        // A feature already on the walk is either being checked further up or already passed, so a cycle does not
        // make it unsatisfiable on its own.
        if (!visited.Add(definition.Id))
            return true;

        return definition.Requires.All(required => IsSatisfiable(required, visited));
    }

    private List<FeatureConflict> FindConflicts(HashSet<string> enabled)
    {
        var conflicts = new List<FeatureConflict>();
        var seen = new HashSet<(string, string)>();
        foreach (string id in enabled.OrderBy(id => id, StringComparer.Ordinal))
        {
            foreach (string conflicting in _features[id].Conflicts)
            {
                if (!_features.TryGetValue(conflicting, out FeatureDefinition other) || !enabled.Contains(other.Id))
                    continue;

                var conflict = new FeatureConflict(id, other.Id);
                if (seen.Add((conflict.First, conflict.Second)))
                {
                    conflicts.Add(conflict);
                }
            }
        }

        return conflicts;
    }

    private static List<string> QualifyAll(string modId, IEnumerable<string> ids)
    {
        var qualified = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string id in ids)
        {
            if (string.IsNullOrEmpty(id))
                continue;

            string full = FeatureIds.Qualify(modId, id);
            if (seen.Add(full))
            {
                qualified.Add(full);
            }
        }

        return qualified;
    }
}
