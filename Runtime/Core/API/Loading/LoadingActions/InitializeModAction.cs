using System;
using ReduxLib.GameInterfaces;
using SpaceWarp2.API.Mods;

namespace SpaceWarp2.Patching.LoadingActions;

/// <summary>
/// Initializes a mod by firing its Lua init hooks and its C# OnInitialized callback.
/// </summary>
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
            // Fire the mod's Lua lifecycle hooks before its C# OnInitialized so script contributions run first.
            // Each hook is isolated so one mod's error does not abort the phase for the rest.
            foreach (var hook in _plugin.InitHooks)
            {
                try
                {
                    hook();
                }
                catch (Exception hookError)
                {
                    _plugin.Logger.LogError(hookError.ToString());
                }
            }

            if (_plugin.DoLoadingActions)
            {
                _plugin.Plugin?.OnInitialized();
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