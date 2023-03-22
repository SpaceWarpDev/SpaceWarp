using System;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class PreInitializeModAction : FlowAction
{
    private readonly BaseSpaceWarpPlugin _plugin;

    public PreInitializeModAction(BaseSpaceWarpPlugin plugin) : base(
        $"Pre-initialization for plugin {plugin.Info.Metadata.Name}")
    {
        _plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            _plugin.OnPreInitialized();
            resolve();
        }
        catch (Exception e)
        {
            _plugin.ModLogger.LogError(e.ToString());
            reject(null);
        }
    }
}