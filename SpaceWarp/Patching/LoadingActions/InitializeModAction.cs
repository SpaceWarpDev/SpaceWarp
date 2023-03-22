using System;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class InitializeModAction : FlowAction
{
    private readonly BaseSpaceWarpPlugin _plugin;

    public InitializeModAction(BaseSpaceWarpPlugin plugin) : base(
        $"Initialization for plugin {plugin.Info.Metadata.Name}")
    {
        _plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            _plugin.OnInitialized();
            resolve();
        }
        catch (Exception e)
        {
            _plugin.ModLogger.LogError(e.ToString());
            reject(null);
        }
    }
}