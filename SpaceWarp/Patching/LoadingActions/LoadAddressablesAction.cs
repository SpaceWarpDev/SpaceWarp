using System;
using System.IO;
using BepInEx.Logging;
using KSP.Game.Flow;
using SpaceWarp.API.Mods;
using SpaceWarp.InternalUtilities;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class LoadAddressablesAction : FlowAction
{
    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Addressables Loader");
    private readonly SpaceWarpPluginDescriptor _plugin;

    public LoadAddressablesAction(SpaceWarpPluginDescriptor plugin) : base(
        $"Loading addressables for {plugin.SWInfo.Name}")
    {
        _plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            var addressablesPath = Path.Combine(_plugin.Folder.FullName, "addressables");
            Logger.LogInfo($"Loading addressables for {_plugin.SWInfo.Name}");
            var catalogPath = Path.Combine(addressablesPath, "catalog.json");
            if (File.Exists(catalogPath))
            {
                Logger.LogInfo($"Found addressables for {_plugin.SWInfo.Name}");
                AssetHelpers.LoadAddressable(catalogPath);
            }
            else
            {
                Logger.LogInfo($"Did not find addressables for {_plugin.SWInfo.Name}");
            }

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