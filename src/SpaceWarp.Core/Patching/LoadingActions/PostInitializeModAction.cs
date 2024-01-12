using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class PostInitializeModAction : FlowAction
{
    private readonly SpaceWarpPluginDescriptor _plugin;

    public PostInitializeModAction(SpaceWarpPluginDescriptor plugin)
        : base($"Post-initialization for plugin {plugin.Name}")
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
            reject(null);
        }
    }
}