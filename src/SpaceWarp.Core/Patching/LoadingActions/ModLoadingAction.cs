using JetBrains.Annotations;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

// TODO: Move this to SpaceWarp.API.Loading in 2.0.0

/// <summary>
/// A general loading action for a SpaceWarp mod plugin.
/// </summary>
[Obsolete("This will be moved to SpaceWarp.API.Loading in 2.0.0")]
[PublicAPI]
public class ModLoadingAction : FlowAction
{
    private Action<BaseSpaceWarpPlugin> _action;
    private BaseSpaceWarpPlugin _plugin;

    /// <summary>
    /// Creates a new mod plugin loading action.
    /// </summary>
    /// <param name="actionName">Name of the loading action.</param>
    /// <param name="action">Callback to perform the loading action.</param>
    /// <param name="plugin">The mod plugin to create the action for.</param>
    public ModLoadingAction(
        string actionName,
        Action<BaseSpaceWarpPlugin> action,
        BaseSpaceWarpPlugin plugin
    ) : base($"{plugin.SpaceWarpMetadata.Name}:{actionName}")
    {
        _action = action;
        _plugin = plugin;
    }

    /// <summary>
    /// Performs the loading action.
    /// </summary>
    /// <param name="resolve">Callback to call when the action is resolved.</param>
    /// <param name="reject">Callback to call when the action is rejected.</param>
    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            _action(_plugin);
            resolve();
        }
        catch (Exception e)
        {
            _plugin.SWLogger.LogError(e.ToString());
            reject(null);
        }
    }
}