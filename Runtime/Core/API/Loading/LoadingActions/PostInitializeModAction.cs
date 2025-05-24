using System;
using ReduxLib.GameInterfaces;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class PostInitializeModAction : BaseFlowAction
{
    private readonly SpaceWarpPluginDescriptor _plugin;

    public PostInitializeModAction(SpaceWarpPluginDescriptor plugin)
        : base($"Post-initialization for plugin {plugin.Name}", "Post-initialization")
    {
        _plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            if (_plugin.DoLoadingActions)
            {
                _plugin.Plugin!.OnPostInitialized();
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