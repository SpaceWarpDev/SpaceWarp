using System;
using System.IO;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class LoadLocalizationAction : FlowAction
{
    private readonly BaseSpaceWarpPlugin Plugin;

    public LoadLocalizationAction(BaseSpaceWarpPlugin plugin) : base($"Loading localizations for plugin {plugin.SpaceWarpMetadata.Name}")
    {
        Plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            string localizationsPath = Path.Combine(Plugin.PluginFolderPath, "localizations");
            AssetHelpers.LoadLocalizationFromFolder(localizationsPath);
            resolve();
        }
        catch (Exception e)
        {
            Plugin.ModLogger.LogError(e.ToString());
            reject(null);
        }
    }
}