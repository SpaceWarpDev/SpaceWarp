using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using ReduxLib.GameInterfaces;
using SpaceWarp2.API.Features;
using SpaceWarp2.InternalUtilities;
using SpaceWarp2.API.Loading.LoadingActions;
using SpaceWarp2.API.Mods;
using SpaceWarp2.Patching.LoadingActions;
using UnityEngine;
using UnityObject = UnityEngine.Object;

#pragma warning disable CS0618 // Type or member is obsolete

namespace SpaceWarp2.API.Loading;

/// <summary>
/// API for mods to register their actions for the loading of assets.
/// </summary>
[PublicAPI]
public static class Loading
{
    public static List<Func<SpaceWarpPluginDescriptor, DescriptorLoadingAction>> DescriptorLoadingActionGenerators =
        new();

    public static List<Func<IFlowAction>> GeneralLoadingActions = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        DescriptorLoadingActionGenerators = new();
        GeneralLoadingActions = new();
        SetActions.Clear();
    }

    // Data mods load per feature set, cleared and loaded again on boot and on every switch to another set, never on
    // a reload into the same set, since the data is the same for every save on one set
    private static readonly List<FeatureGatedAction> SetActions = new();

    /// <summary>
    /// Gets every registered per-set action.
    /// </summary>
    public static IReadOnlyList<FeatureGatedAction> RegisteredSetActions => SetActions;

    /// <summary>
    /// Lists the steps that bring per-set data in line with the active set.
    /// </summary>
    /// <remarks>
    /// Every action resets first, whether or not it loads, so no load reads another mod's half-cleared data and
    /// nothing from the previous set survives in an action that no longer applies.
    /// </remarks>
    /// <param name="actions">The actions, in registration order, normally <see cref="RegisteredSetActions" />.</param>
    /// <returns>Each step's name and work, the work given a callback to call once done.</returns>
    public static List<(string name, Action<Action> work)> SetActionSteps(IEnumerable<FeatureGatedAction> actions)
    {
        var all = new List<FeatureGatedAction>(actions);
        var steps = new List<(string name, Action<Action> work)>();
        foreach (FeatureGatedAction action in all)
        {
            steps.Add(($"Clearing {action.Name}", done =>
            {
                action.Reset?.Invoke();
                done();
            }));
        }

        // Selectors are read as each load starts, which is after the new set has activated
        foreach (FeatureGatedAction action in all)
        {
            steps.Add(($"Loading {action.Name}", done =>
            {
                if (action.Applies())
                {
                    action.Run(done);
                }
                else
                {
                    done();
                }
            }));
        }

        return steps;
    }

    /// <summary>
    /// Registers a per mod loading action (but more general). Should be added either on Awake() or Start().
    /// </summary>
    /// <param name="name">The name of the action</param>
    /// <param name="action">The action</param>
    public static void AddDescriptorLoadingAction(string name, Action<SpaceWarpPluginDescriptor> action)
    {
        DescriptorLoadingActionGenerators.Add(p => new DescriptorLoadingAction(name, action, p));
    }

    /// <summary>
    /// Registers a general loading action. Should be added either on Awake() or Start().
    /// </summary>
    /// <param name="actionGenerator">The action generator</param>
    public static void AddGeneralLoadingAction(Func<IFlowAction> actionGenerator)
    {
        GeneralLoadingActions.Add(actionGenerator);
    }

    /// <summary>
    /// Registers an action to be done on addressables after addressables have been loaded. Should be added either on Awake() or Start().
    /// </summary>
    /// <param name="name">The name of the action</param>
    /// <param name="label">The addressables label to hook into</param>
    /// <param name="action">The action to be done on each addressables asset</param>
    /// <typeparam name="T">The type of asset that this action is done upon</typeparam>
    public static void AddAddressablesLoadingAction<T>(string name, string label, Action<T> action)
        where T : UnityObject
    {
        AddGeneralLoadingAction(() => new AddressableAction<T>(name, label, action));
    }

    /// <summary>
    /// Registers an action to be done on addressables after addressables have been loaded. Should be added either on Awake() or Start().
    /// Allows to keep asset in memory after loading them. This is useful for textures or UXML templates, for example.
    /// </summary>
    /// <param name="name">The name of the action</param>
    /// <param name="label">The addressables label to hook into</param>
    /// <param name="keepAssets">Indicates if assets should be kept in memory after loading them.</param>
    /// <param name="action">The action to be done on each addressables asset</param>
    /// <typeparam name="T">The type of asset that this action is done upon</typeparam>
    public static void AddAddressablesLoadingAction<T>(string name, string label, bool keepAssets, Action<T> action)
        where T : UnityObject
    {
        AddGeneralLoadingAction(() => new AddressableAction<T>(name, label, keepAssets, action));
    }

    /// <summary>
    /// Registers data a mod loads for the active feature set, cleared and loaded again whenever the set changes.
    /// </summary>
    /// <param name="modId">The mod the data belongs to.</param>
    /// <param name="name">What is loaded, shown as "Clearing" and "Loading" it.</param>
    /// <param name="reset">Clears the data loaded for the previous set.</param>
    /// <param name="load">Loads the data for the active set, calling its argument once done.</param>
    /// <returns>The action, for chaining feature selectors.</returns>
    public static FeatureGatedAction AddSetAction(string modId, string name, Action reset, Action<Action> load)
    {
        var action = new FeatureGatedAction(modId, name, load, reset);
        SetActions.Add(action);
        return action;
    }

    /// <summary>
    /// Registers an addressables label a mod loads for the active feature set, so its patched and gated assets
    /// are read again whenever the set changes.
    /// </summary>
    /// <typeparam name="T">The asset type.</typeparam>
    /// <param name="modId">The mod the data belongs to.</param>
    /// <param name="name">The name shown while it loads.</param>
    /// <param name="label">The addressables label.</param>
    /// <param name="onAsset">Called with each loaded asset.</param>
    /// <param name="reset">Clears what the previous set's assets registered.</param>
    /// <returns>The action, for chaining feature selectors.</returns>
    public static FeatureGatedAction AddSetLoadingAction<T>(
        string modId,
        string name,
        string label,
        Action<T> onAsset,
        Action reset
    ) where T : UnityObject =>
        AddSetAction(
            modId,
            name,
            reset,
            done =>
            {
                // No catalog may ship the label at all, which is not an error for data that is optional
                if (!IAssetProvider.Instance.DoesLabelExist(label))
                {
                    done();
                    return;
                }

                IAssetProvider.Instance.LoadByLabel<T>(
                label,
                onAsset,
                assets =>
                {
                    // A set can have nothing under the label, which comes back as no list at all
                    if (assets != null)
                    {
                        UnityEngine.AddressableAssets.Addressables.Release(assets);
                    }

                    done();
                }
            );
            }
        );
}