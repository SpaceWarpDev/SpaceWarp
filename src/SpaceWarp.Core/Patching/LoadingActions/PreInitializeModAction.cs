using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class PreInitializeModAction : FlowAction
{
    private readonly SpaceWarpPluginDescriptor _plugin;

    public PreInitializeModAction(SpaceWarpPluginDescriptor plugin)
        : base($"Pre-initialization for plugin {plugin.Name}")
    {
        _plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        SpaceWarpPlugin.Logger.LogInfo($"Pre-initializing: {_plugin.Name}?");
        try
        {
            if (_plugin.DoLoadingActions)
            {
                SpaceWarpPlugin.Logger.LogInfo($"YES! {_plugin.Plugin}");
                _plugin.Plugin.OnPreInitialized();
            }
            else
            {
                SpaceWarpPlugin.Logger.LogInfo("NO!!");
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