using JetBrains.Annotations;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

// TODO: Move this to SpaceWarp.API.Loading in 2.0.0

[Obsolete("This will be moved to SpaceWarp.API.Loading in 2.0.0")]
[PublicAPI]
public class ModLoadingAction : FlowAction
{
    private Action<BaseSpaceWarpPlugin> _action;
    private BaseSpaceWarpPlugin _plugin;

    public ModLoadingAction(
        string actionName,
        Action<BaseSpaceWarpPlugin> action,
        BaseSpaceWarpPlugin plugin
    ) : base($"{plugin.SpaceWarpMetadata.Name}:{actionName}")
    {
        _action = action;
        _plugin = plugin;
    }

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