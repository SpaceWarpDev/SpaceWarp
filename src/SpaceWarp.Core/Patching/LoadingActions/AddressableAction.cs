using JetBrains.Annotations;
using KSP.Game;
using KSP.Game.Flow;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SpaceWarp.Patching.LoadingActions;

// TODO: Move this to SpaceWarp.API.Loading in 2.0.0

/// <summary>
/// A loading action that loads addressable assets by label.
/// </summary>
/// <typeparam name="T">The type of assets to load.</typeparam>
[Obsolete("This will be moved to SpaceWarp.API.Loading in 2.0.0")]
[PublicAPI]
public class AddressableAction<T> : FlowAction where T : UnityObject
{
    private string _label;
    private Action<T> _action;

    /// <summary>
    /// Creates a new addressable loading action.
    /// </summary>
    /// <param name="name">Name of the action.</param>
    /// <param name="label">Label of the asset to load.</param>
    /// <param name="action">Action to perform on the loaded asset.</param>
    public AddressableAction(string name, string label, Action<T> action) : base(name)
    {
        _label = label;
        _action = action;
    }

    private bool DoesLabelExist(object label)
    {
        return GameManager.Instance.Assets._registeredResourceLocators.Any(locator => locator.Keys.Contains(label))
               || Addressables.ResourceLocators.Any(locator => locator.Keys.Contains(label));
    }

    /// <summary>
    /// Performs the loading action.
    /// </summary>
    /// <param name="resolve">Callback to call when the action is resolved.</param>
    /// <param name="reject">Callback to call when the action is rejected.</param>
    public override void DoAction(Action resolve, Action<string> reject)
    {
        if (!DoesLabelExist(_label))
        {
            Debug.Log($"[Space Warp] Skipping loading addressables for {_label} which does not exist.");
            resolve();
            return;
        }

        try
        {
            GameManager.Instance.Assets.LoadByLabel(_label,_action,delegate(IList<T> assetLocations)
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