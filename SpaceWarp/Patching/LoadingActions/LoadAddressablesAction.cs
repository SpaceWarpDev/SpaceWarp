using System;
using System.IO;
using BepInEx.Logging;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class LoadAddressablesAction : FlowAction
{
    private readonly BaseSpaceWarpPlugin Plugin;
    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Addressables Loader");

    public LoadAddressablesAction(BaseSpaceWarpPlugin plugin) : base($"Loading addressables for {plugin.SpaceWarpMetadata.Name}")
    {
        Plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            string addressablesPath = Path.Combine(Plugin.PluginFolderPath, "addressables");
            Logger.LogInfo($"Loading addressables for {Plugin.SpaceWarpMetadata.Name}");
            string catalogPath = Path.Combine(addressablesPath, "catalog.json");
            if (File.Exists(catalogPath))
            {
                Logger.LogInfo($"Found addressables for {Plugin.SpaceWarpMetadata.Name}");
                AssetHelpers.LoadAddressable(catalogPath);
            }
            else
            {
                Logger.LogInfo($"Did not find addressables for {Plugin.SpaceWarpMetadata.Name}");
            }
            resolve();
        }
        catch (Exception e)
        {
            Plugin.ModLogger.LogError(e.ToString());
            reject(null);
        }
    }
}