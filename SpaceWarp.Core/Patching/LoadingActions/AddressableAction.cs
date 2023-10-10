using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using KSP.Game;
using KSP.Game.Flow;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SpaceWarp.Patching.LoadingActions;

[Obsolete("This will be moved to SpaceWarp.API.Loading in 2.0.0")]
[PublicAPI]
public class AddressableAction<T> : FlowAction where T : UnityObject
{
    private string Label;
    private Action<T> Action;

    public AddressableAction(string name, string label, Action<T> action) : base(name)
    {
        Label = label;
        Action = action;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            GameManager.Instance.Assets.LoadByLabel(Label,Action,delegate(IList<T> assetLocations)
            {
                if (assetLocations != null)
                {
                    Addressables.Release(assetLocations);
                }

                resolve();
            });
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            reject(null);
        }
    }
}