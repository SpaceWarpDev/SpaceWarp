using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using ReduxLib.GameInterfaces;
using SpaceWarp.API.Loading.LoadingActions;
using SpaceWarp.API.Mods;
using SpaceWarp.InternalUtilities;
using SpaceWarp.Patching.LoadingActions;
using UnityObject = UnityEngine.Object;

#pragma warning disable CS0618 // Type or member is obsolete

namespace SpaceWarp.API.Loading;

/// <summary>
/// API for mods to register their actions for the loading of assets.
/// </summary>
[PublicAPI]
public static class Loading
{
    public static List<Func<SpaceWarpPluginDescriptor, DescriptorLoadingAction>> DescriptorLoadingActionGenerators =
        new();

    public static List<Func<IFlowAction>> GeneralLoadingActions = new();

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
}