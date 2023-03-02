using System;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class PostInitializeModAction : FlowAction
{
    private BaseSpaceWarpPlugin Plugin;

    public PostInitializeModAction(BaseSpaceWarpPlugin plugin) : base($"Post-initialization for plugin {plugin.Info.Metadata.Name}")
    {
        Plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            Plugin.OnPostInitialized();
            resolve();
        }
        catch (Exception e)
        {
            Plugin.ModLogger.LogError(e.ToString());
            reject(null);
        }
    }
}
