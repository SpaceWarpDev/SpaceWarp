using System;
using System.IO;
using BepInEx.Logging;
using KSP.Game.Flow;
using SpaceWarp.API.Assets;
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
            string bundlesPath = Path.Combine(Plugin.PluginFolderPath, GlobalModDefines.AssetBundlesFolder);
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
                Logger.LogInfo($"Did not load asset bundles for {Plugin.SpaceWarpMetadata.Name} as no asset bundles folder existed!");
            }

            string imagesPath = Path.Combine(Plugin.PluginFolderPath, GlobalModDefines.ImageAssetsFolder);
            if (Directory.Exists(imagesPath))
            {
                var directoryInfo = new DirectoryInfo(imagesPath);
                foreach (string file in directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Select(fileInfo => fileInfo.FullName))
                {
                    var assetPathList = PathHelpers.GetRelativePath(imagesPath, file).Split(Path.DirectorySeparatorChar);
                    //We have to make sure it uses '/' as the path separator and toLower() the names
                    var assetPath = "";
                    for (int i = 0; i < assetPathList.Length; i++)
                    {
                        assetPath += assetPathList[i].ToLower();
                        if (i != assetPathList.Length - 1)
                        {
                            assetPath += "/";
                        }
                    }

                    assetPath = $"images/{assetPath}";
                    
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false)
                    {
                        filterMode = FilterMode.Point
                    };
                    try
                    {
                        var fileData = File.ReadAllBytes(file);
                        tex.LoadImage(fileData); // Will automatically resize
                        AssetManager.RegisterSingleAsset(Plugin.SpaceWarpMetadata.ModID,assetPath,tex);
                    }
                    catch (Exception e)
                    {
                        Plugin.ModLogger.LogError(e.ToString());
                    }
                }
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