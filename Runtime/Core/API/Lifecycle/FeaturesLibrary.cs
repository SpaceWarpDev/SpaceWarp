using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using SpaceWarp2.API.Features;

namespace SpaceWarp2.API.Lifecycle;

/// <summary>
/// The <c>Features</c> global: a read-only view of the active feature set.
/// </summary>
[PublicAPI]
[MoonSharpUserData]
public class FeaturesLibrary
{
    /// <summary>
    /// Gets the one instance every environment shares.
    /// </summary>
    [MoonSharpHidden]
    public static readonly FeaturesLibrary Instance = new();

    static FeaturesLibrary()
    {
        UserData.RegisterType<FeaturesLibrary>();
    }

    /// <summary>
    /// Checks whether a feature is on.
    /// </summary>
    /// <param name="context">The calling script, whose <c>ModId</c> a bare id belongs to.</param>
    /// <param name="featureId">The feature id, bare or qualified.</param>
    /// <returns>True if the feature is on.</returns>
    public bool Has(ScriptExecutionContext context, string featureId) => false;

    /// <summary>
    /// Lists the enabled features as a set.
    /// </summary>
    /// <param name="context">The calling script.</param>
    /// <returns>A new table with each enabled id as a key mapped to true, or nil before any set is active.</returns>
    public DynValue All(ScriptExecutionContext context) => DynValue.Nil;
}

/// <summary>
/// Keeps each mod environment's <c>Enabled</c> in step with the active set.
/// </summary>
[PublicAPI]
public static class FeatureEnvironments
{
    // Environments are forked per mod and per console run, so they are held weakly and dropped once collected
    private static readonly List<(WeakReference<Table> env, string modId)> Tracked = new();

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Tracked.Clear();
    }

    /// <summary>
    /// Starts keeping an environment's <c>Enabled</c> up to date, and sets it now.
    /// </summary>
    /// <param name="env">The environment's globals.</param>
    /// <param name="modId">The mod the environment belongs to.</param>
    public static void Track(Table env, string modId)
    {
    }

    /// <summary>
    /// Checks whether a mod's runtime hooks run in the active feature set.
    /// </summary>
    /// <remarks>
    /// The console is not a mod, so its hooks always run.
    /// </remarks>
    /// <param name="modId">The id the hooks were registered under.</param>
    /// <returns>True if the hooks run.</returns>
    public static bool RunsHooksOf(string modId) => true;

    internal static void Refresh()
    {
    }
}
