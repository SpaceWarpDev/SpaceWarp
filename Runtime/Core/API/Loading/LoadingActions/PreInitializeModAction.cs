using System;
using ReduxLib.GameInterfaces;
using SpaceWarp2.API.Mods;

namespace SpaceWarp2.Patching.LoadingActions;

/// <summary>
/// Pre-initializes a mod by firing its C# OnPreInitialized callback.
/// </summary>
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
                _plugin.Plugin?.OnPreInitialized();
            }
            resolve();
        }
        catch (Exception e)
        {
            _plugin.Logger.LogError(e.ToString());
            reject(e.ToString());
        }
    }
}