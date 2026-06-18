using System;
using System.IO;
using ReduxLib.GameInterfaces;
using SpaceWarp2.InternalUtilities;
using SpaceWarp2.API.Mods;

namespace SpaceWarp2.Patching.LoadingActions;

/// <summary>
/// Loads a mod's localizations from its localizations folder.
/// </summary>
internal sealed class LoadLocalizationAction : BaseFlowAction
{
    private readonly SpaceWarpPluginDescriptor _plugin;

    public LoadLocalizationAction(SpaceWarpPluginDescriptor plugin)
        : base($"Loading localizations for plugin {plugin.SWInfo.Name}", "Loading localizations")
    {
        _plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            var localizationsPath = Path.Combine(_plugin.Folder.FullName, "localizations");
            AssetHelpers.LoadLocalizationFromFolder(localizationsPath);
            resolve();
        }
        catch (Exception e)
        {
            _plugin.Logger.LogError(e.ToString());
            reject(e.ToString());
        }
    }
}