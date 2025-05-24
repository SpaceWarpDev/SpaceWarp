using System;
using ReduxLib.GameInterfaces;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class PreInitializeModAction : BaseFlowAction
{
    private readonly SpaceWarpPluginDescriptor _plugin;

    public PreInitializeModAction(SpaceWarpPluginDescriptor plugin)
        : base($"Pre-initialization for plugin {plugin.Name}", "Pre-initialization")
    {
        _plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            if (_plugin.DoLoadingActions)
            {
                SpaceWarpPlugin.Instance.SWLogger.LogInfo($"Pre-initializing: {_plugin.Name}?");
                _plugin.Plugin.OnPreInitialized();
            }
            resolve();
        }
        catch (Exception e)
        {
            (_plugin.Plugin ?? SpaceWarpPlugin.Instance).SWLogger.LogError(e.ToString());
            reject(null);
        }
    }
}