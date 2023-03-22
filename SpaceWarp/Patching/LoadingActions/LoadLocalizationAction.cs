using System;
using System.IO;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class LoadLocalizationAction : FlowAction
{
    private readonly BaseSpaceWarpPlugin _plugin;

    public LoadLocalizationAction(BaseSpaceWarpPlugin plugin) : base(
        $"Loading localizations for plugin {plugin.SpaceWarpMetadata.Name}")
    {
        _plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            var localizationsPath = Path.Combine(_plugin.PluginFolderPath, "localizations");
            AssetHelpers.LoadLocalizationFromFolder(localizationsPath);
            resolve();
        }
        catch (Exception e)
        {
            _plugin.ModLogger.LogError(e.ToString());
            reject(null);
        }
    }
}