using System;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

public class DescriptorLoadingAction : FlowAction
{
    private readonly Action<SpaceWarpPluginDescriptor> _action;
    private readonly SpaceWarpPluginDescriptor _plugin;
    
    
    public DescriptorLoadingAction(string actionName, Action<SpaceWarpPluginDescriptor> action, SpaceWarpPluginDescriptor plugin) : base(
        $"{plugin.SWInfo.Name}: {actionName}")
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
            if (_plugin.Plugin != null)
                _plugin.Plugin.ModLogger.LogError(e.ToString());
            else
                SpaceWarpPlugin.Logger.LogError(_plugin.SWInfo.Name + ": " + e);   
            reject(null);
        }
    }
}