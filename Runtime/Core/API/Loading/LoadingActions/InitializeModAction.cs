using System;
using ReduxLib.GameInterfaces;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class InitializeModAction : BaseFlowAction
{
    private readonly SpaceWarpPluginDescriptor _plugin;

    public InitializeModAction(SpaceWarpPluginDescriptor plugin) : base($"Initialization for plugin {plugin.Name}", $"Initializing {plugin.Name}")
    {
        _plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            if (_plugin.DoLoadingActions)
            {
                _plugin.Plugin?.OnInitialized();
            }

            resolve();
        }
        catch (Exception e)
        {
            (_plugin.Plugin ?? SpaceWarpPlugin.Instance).SWLogger.LogError(e.ToString());
            reject(e.ToString());
        }
    }
}