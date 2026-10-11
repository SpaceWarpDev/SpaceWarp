using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using SpaceWarp2.API.Features;

namespace SpaceWarp2.API.Loading;

/// <summary>
/// Data a mod loads for the active feature set: cleared whenever the set changes, and loaded again when the mod
/// and its selected features are on in the new set.
/// </summary>
/// <remarks>
/// Runs at boot for the main menu set and again on every switch to another set, never on a reload into the same
/// set, since the data is the same for every save on one set.
/// </remarks>
[PublicAPI]
[MoonSharpUserData]
public class SetLoadingAction
{
    private readonly List<string> _needs = new();
    private readonly List<string> _conflicts = new();

    /// <summary>
    /// Creates an action without registering it.
    /// </summary>
    /// <param name="modId">The mod the action belongs to.</param>
    /// <param name="name">The name shown while it loads.</param>
    /// <param name="reset">Clears the data loaded for the previous set.</param>
    /// <param name="load">Loads the data for the active set, calling its argument once done.</param>
    public SetLoadingAction(string modId, string name, Action reset, Action<Action> load)
    {
        ModId = modId;
        Name = name;
        Reset = reset;
        Load = load;
    }

    /// <summary>
    /// Gets the mod the action belongs to, whose default feature must be on for it to load.
    /// </summary>
    public string ModId { get; }

    /// <summary>
    /// Gets the name shown while the action loads.
    /// </summary>
    public string Name { get; }

    [MoonSharpHidden]
    internal Action Reset { get; }

    [MoonSharpHidden]
    internal Action<Action> Load { get; }

    /// <summary>
    /// Loads only while a feature is on.
    /// </summary>
    /// <param name="featureId">The feature id, bare ids belonging to the action's mod.</param>
    /// <returns>This action, for chaining.</returns>
    public SetLoadingAction Feature(string featureId) => this;

    /// <summary>
    /// Loads only while a feature is off.
    /// </summary>
    /// <param name="featureId">The feature id, bare ids belonging to the action's mod.</param>
    /// <returns>This action, for chaining.</returns>
    public SetLoadingAction NoFeature(string featureId) => this;

    /// <summary>
    /// Checks whether the action loads in the active set.
    /// </summary>
    /// <returns>True if the mod and every selected feature allow it.</returns>
    public bool Applies() => true;
}

/// <summary>
/// The registered <see cref="SetLoadingAction" />s, and the order a set activation runs them in.
/// </summary>
[PublicAPI]
public static class SetLoading
{
    private static readonly List<SetLoadingAction> Registered = new();

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Registered.Clear();
    }

    /// <summary>
    /// Gets every registered action.
    /// </summary>
    public static IReadOnlyList<SetLoadingAction> Actions => Registered;

    /// <summary>
    /// Registers an action.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <returns>The action, for chaining selectors.</returns>
    public static SetLoadingAction Add(SetLoadingAction action)
    {
        Registered.Add(action);
        return action;
    }

    /// <summary>
    /// Lists the steps that bring every action's data in line with the active set.
    /// </summary>
    /// <remarks>
    /// Every action resets first, whether or not it loads, so no load reads another mod's half-cleared data and
    /// nothing from the previous set survives in an action that no longer applies.
    /// </remarks>
    /// <param name="actions">The actions, in registration order.</param>
    /// <returns>Each step's name and work, the work given a callback to call once done.</returns>
    public static List<(string name, Action<Action> work)> Steps(IEnumerable<SetLoadingAction> actions) => new();
}
