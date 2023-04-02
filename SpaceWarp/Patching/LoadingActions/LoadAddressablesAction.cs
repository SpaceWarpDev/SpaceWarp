using System;
using System.IO;
using BepInEx.Logging;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class LoadAddressablesAction : FlowAction
{
    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Addressables Loader");
    private readonly BaseSpaceWarpPlugin _plugin;

    public LoadAddressablesAction(BaseSpaceWarpPlugin plugin) : base(
        $"Loading addressables for {plugin.SpaceWarpMetadata.Name}")
    {
        _plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            var addressablesPath = Path.Combine(_plugin.PluginFolderPath, "addressables");
            Logger.LogInfo($"Loading addressables for {_plugin.SpaceWarpMetadata.Name}");
            var catalogPath = Path.Combine(addressablesPath, "catalog.json");
            if (File.Exists(catalogPath))
            {
                Logger.LogInfo($"Found addressables for {_plugin.SpaceWarpMetadata.Name}");
                AssetHelpers.LoadAddressable(catalogPath);
            }
            else
            {
                Logger.LogInfo($"Did not find addressables for {_plugin.SpaceWarpMetadata.Name}");
            }

            resolve();
        }
        catch (Exception e)
        {
            _plugin.ModLogger.LogError(e.ToString());
            reject(null);
        }
    }
}