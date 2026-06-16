using System;
using ReduxLib.GameInterfaces;
using SpaceWarp2.API.Mods;

namespace SpaceWarp2.Patching.LoadingActions;

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
            // Fire the mod's Lua PostInit hooks before its C# OnPostInitialized. Isolated per hook.
            foreach (var hook in _plugin.PostInitHooks)
            {
                try
                {
                    hook();
                }
                catch (Exception hookError)
                {
                    (_plugin.Plugin ?? SpaceWarpPlugin.Instance).SWLogger.LogError(hookError.ToString());
                }
            }

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