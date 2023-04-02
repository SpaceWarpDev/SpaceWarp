using System;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class PostInitializeModAction : FlowAction
{
    private readonly BaseSpaceWarpPlugin _plugin;

    public PostInitializeModAction(BaseSpaceWarpPlugin plugin) : base(
        $"Post-initialization for plugin {plugin.Info.Metadata.Name}")
    {
        _plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            _plugin.OnPostInitialized();
            resolve();
        }
        catch (Exception e)
        {
            _plugin.ModLogger.LogError(e.ToString());
            reject(null);
        }
    }
}