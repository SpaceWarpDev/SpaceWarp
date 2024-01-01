using JetBrains.Annotations;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

[Obsolete("This will be moved to SpaceWarp.API.Loading in 2.0.0")]
[PublicAPI]
public class ModLoadingAction : FlowAction
{
    private Action<BaseSpaceWarpPlugin> Action;
    private BaseSpaceWarpPlugin Plugin;


    public ModLoadingAction(
        string actionName,
        Action<BaseSpaceWarpPlugin> action,
        BaseSpaceWarpPlugin plugin
    ) : base($"{plugin.SpaceWarpMetadata.Name}:{actionName}")
    {
        Action = action;
        Plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            Action(Plugin);
            resolve();
        }
        catch (Exception e)
        {
            Plugin.SWLogger.LogError(e.ToString());
            reject(null);
        }
    }
}