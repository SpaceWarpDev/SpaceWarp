using JetBrains.Annotations;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

// TODO: Move this to SpaceWarp.API.Loading in 2.0.0

/// <summary>
/// A general loading action for a mod descriptor.
/// </summary>
[Obsolete("This will be moved to SpaceWarp.API.Loading in 2.0.0")]
[PublicAPI]
public class DescriptorLoadingAction : FlowAction
{
    private readonly Action<SpaceWarpPluginDescriptor> _action;
    private readonly SpaceWarpPluginDescriptor _plugin;

    /// <summary>
    /// Creates a new descriptor loading action.
    /// </summary>
    /// <param name="actionName">Name of the loading action.</param>
    /// <param name="action">Callback to perform the loading action.</param>
    /// <param name="plugin">The plugin descriptor to create the action for.</param>
    public DescriptorLoadingAction(
        string actionName,
        Action<SpaceWarpPluginDescriptor> action,
        SpaceWarpPluginDescriptor plugin
    ) : base($"{plugin.SWInfo.Name}: {actionName}")
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
            if (_plugin.DoLoadingActions)
            {
                _action(_plugin);
            }

            resolve();
        }
        catch (Exception e)
        {
            if (_plugin.Plugin != null)
            {
                _plugin.Plugin.SWLogger.LogError(e.ToString());
            }
            else
            {
                SpaceWarpPlugin.Logger.LogError(_plugin.SWInfo.Name + ": " + e);
            }

            reject(null);
        }
    }
}