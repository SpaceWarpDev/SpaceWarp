using System;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class InitializeModAction : FlowAction
{
    private readonly SpaceWarpPluginDescriptor _plugin;

    public InitializeModAction(SpaceWarpPluginDescriptor plugin) : base(
        $"Initialization for plugin {plugin.Name}")
    {
        _plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            _plugin.Plugin.OnInitialized();
            resolve();
        }
        catch (Exception e)
        {
            _plugin.Plugin.SWLogger.LogError(e.ToString());
            reject(null);
        }
    }
}