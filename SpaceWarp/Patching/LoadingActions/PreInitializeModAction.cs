using System;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class PreInitializeModAction : FlowAction
{
    private BaseSpaceWarpPlugin Plugin;

    public PreInitializeModAction(BaseSpaceWarpPlugin plugin) : base($"Pre-initialization for plugin {plugin.Info.Metadata.Name}")
    {
        Plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            Plugin.OnPreInitialized();
            resolve();
        }
        catch (Exception e)
        {
            Plugin.ModLogger.LogError(e.ToString());
            reject(null);
        }
    }
}
