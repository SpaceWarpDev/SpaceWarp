using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using ReduxLib.GameInterfaces;
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
    /// <param name="name">The name shown while it loads.</param>
    /// <param name="reset">Clears the data loaded for the previous set.</param>
    /// <param name="load">Loads the data for the active set, calling its argument once done.</param>
    /// <returns>The action, for chaining feature selectors.</returns>
    public static SetLoadingAction AddSetAction(string modId, string name, Action reset, Action<Action> load) =>
        SetLoading.Add(new SetLoadingAction(modId, name, reset, load));

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
    public static SetLoadingAction AddSetLoadingAction<T>(
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
            done => IAssetProvider.Instance.LoadByLabel<T>(
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
            )
        );
}