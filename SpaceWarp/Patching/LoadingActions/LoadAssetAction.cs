using System;
using System.IO;
using BepInEx.Logging;
using KSP.Game.Flow;
using SpaceWarp.API.AssetBundles;
using SpaceWarp.API.Mods;
using UnityEngine;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class LoadAssetAction : FlowAction
{
    private readonly BaseSpaceWarpPlugin Plugin;
    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Asset Loader");

    public LoadAssetAction(BaseSpaceWarpPlugin plugin) : base($"Loading {plugin.SpaceWarpMetadata.Name} assets")
    {
        Plugin = plugin;
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            string bundlesPath = Path.Combine(Plugin.PluginFolderPath, GlobalModDefines.ASSET_BUNDLES_FOLDER);
            if (Directory.Exists(bundlesPath))
            {
                foreach (string file in Directory.GetFiles(bundlesPath))
                {
                    string assetBundleName = Path.GetFileNameWithoutExtension(file);
                    if (Path.GetExtension(file) != ".bundle") continue;
                    

                    AssetBundle assetBundle = AssetBundle.LoadFromFile(file);

                    if (assetBundle == null)
                    {
                        Logger.LogError($"Failed to load AssetBundle {Plugin.SpaceWarpMetadata.ModID}/{assetBundleName}");
                        continue;
                    }
                    AssetManager.RegisterAssetBundle(Plugin.SpaceWarpMetadata.ModID, assetBundleName, assetBundle);
                    Logger.LogInfo($"Loaded AssetBundle {Plugin.SpaceWarpMetadata.ModID}/{assetBundleName}");
                }
            }
            else
            {
                Logger.LogInfo($"Did not load assets for {Plugin.SpaceWarpMetadata.Name} as no assets folder existed!");
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