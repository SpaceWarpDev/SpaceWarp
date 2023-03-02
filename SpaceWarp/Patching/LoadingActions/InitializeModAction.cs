using System;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class InitializeModAction : FlowAction
{
    private BaseSpaceWarpPlugin Plugin;

    public InitializeModAction(BaseSpaceWarpPlugin plugin) : base($"Initialization for plugin {plugin.Info.Metadata.Name}")
    {
        Plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            Plugin.OnInitialized();
            resolve();
        }
        catch (Exception e)
        {
            Plugin.ModLogger.LogError(e.ToString());
            reject(null);
        }
    }
}
