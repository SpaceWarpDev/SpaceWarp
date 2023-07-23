using System;
using System.IO;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;
using SpaceWarp.InternalUtilities;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class LoadLocalizationAction : FlowAction
{
    private readonly SpaceWarpPluginDescriptor _plugin;

    public LoadLocalizationAction(SpaceWarpPluginDescriptor plugin) : base(
        $"Loading localizations for plugin {plugin.SWInfo.Name}")
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
            if (_plugin.Plugin != null)
                _plugin.Plugin.SWLogger.LogError(e.ToString());
            else
                SpaceWarpPlugin.Logger.LogError(_plugin.SWInfo.Name + ": " + e);   
            reject(null);
        }
    }
}