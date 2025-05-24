using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ReduxLib.GameInterfaces;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityObject = UnityEngine.Object;

namespace SpaceWarp.API.Loading.LoadingActions;


// TODO: Move this to SpaceWarp.API.Loading in 2.0.0

/// <summary>
/// A loading action that loads addressable assets by label.
/// </summary>
/// <typeparam name="T">The type of assets to load.</typeparam>
[PublicAPI]
public class AddressableAction<T> : BaseFlowAction where T : UnityObject
{
    private string _label;
    private Action<T> _action;
    private bool _keepAssets;

    /// <summary>
    /// Creates a new addressable loading action.
    /// </summary>
    /// <param name="name">Name of the action.</param>
    /// <param name="label">Label of the asset to load.</param>
    /// <param name="action">Action to perform on the loaded asset.</param>
    public AddressableAction(string name, string label, Action<T> action) : base(name,name)
    {
        _label = label;
        _action = action;
    }
    
    /// <summary>
    /// Creates a new addressable loading action, with the option to keep the asset in memory after loading.
    /// This is useful for textures or UXML templates, for example.
    /// </summary>
    /// <param name="name">Name of the action.</param>
    /// <param name="label">Label of the asset to load.</param>
    /// <param name="action">Action to perform on the loaded asset.</param>
    /// <param name="keepAssets">Allows to keep asset in memory after loading them.</param>
    public AddressableAction(string name, string label, bool keepAssets, Action<T> action) : this(name, label, action)
    {
        _keepAssets = keepAssets;
    }

    private bool DoesLabelExist(string label) => IAssetProvider.Instance.DoesLabelExist(label);

    /// <summary>
    /// Performs the loading action.
    /// </summary>
    /// <param name="resolve">Callback to call when the action is resolved.</param>
    /// <param name="reject">Callback to call when the action is rejected.</param>
    public override void DoAction(Action resolve, Action<string> reject)
    {
        if (!DoesLabelExist(_label))
        {
            SpaceWarpPlugin.Instance.SWLogger.LogInfo($"Skipping loading addressables for {_label} which does not exist.");
            resolve();
            return;
        }

        try
        {
            IAssetProvider.Instance.LoadByLabel(_label,_action,delegate(IList<T> assetLocations)
            {
                if (assetLocations != null && !_keepAssets)
                {
                    Addressables.Release(assetLocations);
                }

                resolve();
            });
        }
        catch (Exception e)
        {
            SpaceWarpPlugin.Instance.SWLogger.LogError(e.ToString());
            reject(e.ToString());
        }
    }
}