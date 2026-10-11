using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using SpaceWarp2.API.Features;

namespace SpaceWarp2.API.Loading;

/// <summary>
/// Where in loading a campaign a <see cref="CampaignAction" /> runs.
/// </summary>
[PublicAPI]
public enum CampaignPhase
{
    /// <summary>
    /// Right after the campaign's feature set activates, before any of its content loads.
    /// </summary>
    Content,

    /// <summary>
    /// After the parts are read, before the universe is created.
    /// </summary>
    BeforeUniverse,

    /// <summary>
    /// Once the campaign has finished loading.
    /// </summary>
    Loaded
}

/// <summary>
/// What a <see cref="CampaignAction" /> is given while a campaign loads.
/// </summary>
[PublicAPI]
public class CampaignContext
{
    private readonly Action<Action> _addUnload;

    internal CampaignContext(Action<Action> addUnload)
    {
        _addUnload = addUnload;
    }

    /// <summary>
    /// Registers teardown for what this load set up, run when the campaign unloads.
    /// </summary>
    /// <remarks>
    /// Unloads run in the reverse order they were registered, on quitting to the main menu, before any other load
    /// replaces the campaign, and when the load fails part way.
    /// </remarks>
    /// <param name="unload">The teardown.</param>
    public void OnUnload(Action unload)
    {
        _addUnload(unload);
    }
}

/// <summary>
/// Work a mod does every time a campaign loads, gated on the mod and selected features being on in the campaign's
/// set, with teardown registered through its <see cref="CampaignContext" />.
/// </summary>
/// <remarks>
/// Runs on every load, a quickload or revert into the same set included, unlike per-set data.
/// </remarks>
[PublicAPI]
[MoonSharpUserData]
public class CampaignAction
{
    private readonly List<string> _needs = new();
    private readonly List<string> _conflicts = new();

    // Lua registrations hand the action back for chaining selectors
    static CampaignAction()
    {
        UserData.RegisterType<CampaignAction>();
    }

    /// <summary>
    /// Creates an action without registering it.
    /// </summary>
    /// <param name="modId">The mod the action belongs to.</param>
    /// <param name="name">The name shown while it runs.</param>
    /// <param name="phase">Where in loading it runs.</param>
    /// <param name="load">The work, calling its second argument once done.</param>
    public CampaignAction(string modId, string name, CampaignPhase phase, Action<CampaignContext, Action> load)
    {
        ModId = modId;
        Name = name;
        Phase = phase;
        Load = load;
    }

    /// <summary>
    /// Gets the mod the action belongs to.
    /// </summary>
    public string ModId { get; }

    /// <summary>
    /// Gets the name shown while the action runs.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets where in loading the action runs.
    /// </summary>
    public CampaignPhase Phase { get; }

    [MoonSharpHidden]
    internal Action<CampaignContext, Action> Load { get; }

    /// <summary>
    /// Runs only while a feature is on.
    /// </summary>
    /// <param name="featureId">The feature id, bare ids belonging to the action's mod.</param>
    /// <returns>This action, for chaining.</returns>
    public CampaignAction Feature(string featureId)
    {
        _needs.Add(FeatureIds.Qualify(ModId, featureId));
        return this;
    }

    /// <summary>
    /// Runs only while a feature is off.
    /// </summary>
    /// <param name="featureId">The feature id, bare ids belonging to the action's mod.</param>
    /// <returns>This action, for chaining.</returns>
    public CampaignAction NoFeature(string featureId)
    {
        _conflicts.Add(FeatureIds.Qualify(ModId, featureId));
        return this;
    }

    /// <summary>
    /// Checks whether the action runs in the active set.
    /// </summary>
    /// <returns>True if the mod and every selected feature allow it.</returns>
    public bool Applies() =>
        ActiveFeatures.IsModEnabled(ModId)
        && _needs.TrueForAll(ActiveFeatures.IsEnabled)
        && !_conflicts.Exists(ActiveFeatures.IsEnabled);
}

/// <summary>
/// Implemented by a mod's plugin to hear when a campaign it is enabled in loads and unloads.
/// </summary>
[PublicAPI]
public interface ICampaignLifecycle
{
    /// <summary>
    /// Called once a campaign the mod is enabled in has loaded.
    /// </summary>
    void OnCampaignLoaded();

    /// <summary>
    /// Called before that campaign unloads, to undo what <see cref="OnCampaignLoaded" /> set up.
    /// </summary>
    void OnCampaignUnloading();
}

/// <summary>
/// The registered <see cref="CampaignAction" />s, and the unloads owed for the campaign loaded now.
/// </summary>
[PublicAPI]
public static class CampaignLifecycle
{
    private static readonly List<CampaignAction> Registered = new();
    private static readonly List<Action> OwedUnloads = new();

    // A play session that ended without unloading leaves nothing for the next one to tear down
    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Registered.Clear();
        OwedUnloads.Clear();
    }

    /// <summary>
    /// Gets every registered action.
    /// </summary>
    public static IReadOnlyList<CampaignAction> Actions => Registered;

    /// <summary>
    /// Registers an action.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <returns>The action, for chaining selectors.</returns>
    public static CampaignAction Add(CampaignAction action)
    {
        Registered.Add(action);
        return action;
    }

    /// <summary>
    /// Lists the steps of one phase of loading a campaign.
    /// </summary>
    /// <remarks>
    /// Each step decides whether its action applies when it runs, which is after the campaign's set activated.
    /// </remarks>
    /// <param name="phase">The phase.</param>
    /// <param name="actions">The actions, in registration order.</param>
    /// <returns>Each step's name and work, the work given a callback to call once done.</returns>
    public static List<(string name, Action<Action> work)> Steps(CampaignPhase phase, IEnumerable<CampaignAction> actions)
    {
        var steps = new List<(string name, Action<Action> work)>();
        foreach (CampaignAction action in actions)
        {
            if (action.Phase != phase)
                continue;

            steps.Add((action.Name, done =>
            {
                if (!action.Applies())
                {
                    done();
                    return;
                }

                try
                {
                    action.Load(new CampaignContext(OwedUnloads.Add), done);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"[SpaceWarp] Campaign load action {action.Name} of {action.ModId} failed: {e}");
                    done();
                }
            }));
        }

        return steps;
    }

    /// <summary>
    /// Tells each enabled plugin that the campaign has loaded, and owes it the matching unload.
    /// </summary>
    /// <param name="plugins">Each plugin with its mod id.</param>
    public static void NotifyLoaded(IEnumerable<(string modId, ICampaignLifecycle plugin)> plugins)
    {
        foreach ((string modId, ICampaignLifecycle plugin) in plugins)
        {
            if (!ActiveFeatures.IsModEnabled(modId))
                continue;

            try
            {
                plugin.OnCampaignLoaded();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[SpaceWarp] {modId} failed to load its campaign state: {e}");
            }

            OwedUnloads.Add(plugin.OnCampaignUnloading);
        }
    }

    /// <summary>
    /// Runs every unload owed for the campaign loaded now, latest first, and forgets them.
    /// </summary>
    public static void Unload()
    {
        // Taken out first, so an unload that triggers another unload finds nothing left to run twice
        var owed = new List<Action>(OwedUnloads);
        OwedUnloads.Clear();
        for (int i = owed.Count - 1; i >= 0; i--)
        {
            try
            {
                owed[i]();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[SpaceWarp] A campaign unload failed: {e}");
            }
        }
    }
}
