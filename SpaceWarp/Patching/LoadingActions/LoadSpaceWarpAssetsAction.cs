using System;
using System.IO;
using BepInEx.Logging;
using KSP.Game.Flow;
using SpaceWarp.API.AssetBundles;
using SpaceWarp.API.Mods;
using UnityEngine;

namespace SpaceWarp.Patching.LoadingActions;

internal sealed class SpaceWarpAssetInitializationAction : FlowAction
{
    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Space Warp Asset Loader");
    
    public SpaceWarpAssetInitializationAction() : base("Initializing Space Warp Provided Assets")
    {
    }

    public override void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            string bundlesPath = Path.Combine(SpaceWarpManager.SpaceWarpFolder, GlobalModDefines.ASSET_BUNDLES_FOLDER);
        
            if (Directory.Exists(bundlesPath))
            {
                foreach (string file in Directory.GetFiles(bundlesPath))
                {
                    Logger.LogInfo($"Found space warp asset file {file}");
                    string assetBundleName = Path.GetFileNameWithoutExtension(file);
                    if (Path.GetExtension(file) != ".bundle") continue;
                    
                    AssetBundle assetBundle = AssetBundle.LoadFromFile(file);

                    if (assetBundle == null)
                    {
                        Logger.LogError($"Failed to load Space Warp AssetBundle {assetBundleName}");
                        continue;
                    }
                    AssetManager.RegisterAssetBundle("space_warp", assetBundleName, assetBundle);
                    Logger.LogInfo($"Loaded Space Warp AssetBundle {assetBundleName}");
                }
            }
            resolve();
        }
        catch (Exception e)
        {
            SpaceWarpManager.Logger.LogError(e.ToString());
            reject(null);
        }
    }
}