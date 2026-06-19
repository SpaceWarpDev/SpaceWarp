using System;
using System.IO;
using ReduxLib.GameInterfaces;
using ReduxLib.Logging;
using SpaceWarp2.InternalUtilities;
using SpaceWarp2.API.Mods;

namespace SpaceWarp2.Patching.LoadingActions;

/// <summary>
/// Loads a mod's addressables catalog when its addressables folder contains one.
/// </summary>
internal sealed class LoadAddressablesAction : BaseFlowAction
{
    private static readonly ILogger Logger = ReduxLib.ReduxLib.GetLogger("Addressables Loader");
    private readonly SpaceWarpPluginDescriptor _plugin;

    public LoadAddressablesAction(SpaceWarpPluginDescriptor plugin) : base(
        $"Loading addressables for {plugin.SWInfo.Name}", "Loading addressables")
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
            _plugin.Logger.LogError(e.ToString());
            reject(e.ToString());
        }
    }
}