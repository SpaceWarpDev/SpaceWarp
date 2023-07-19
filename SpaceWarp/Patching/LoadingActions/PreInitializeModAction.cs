using System;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class PreInitializeModAction : FlowAction
{
    private readonly SpaceWarpPluginDescriptor _plugin;

    public PreInitializeModAction(SpaceWarpPluginDescriptor plugin) : base(
        $"Pre-initialization for plugin {plugin.Name}")
    {
        _plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            _plugin.Plugin?.OnPreInitialized();
            resolve();
        }
        catch (Exception e)
        {
            _plugin.Plugin?.SWLogger.LogError(e.ToString());
            reject(null);
        }
    }
}