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
    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Asset Loader");
    private readonly BaseSpaceWarpPlugin _plugin;

    public LoadAssetAction(BaseSpaceWarpPlugin plugin) : base($"Loading {plugin.SpaceWarpMetadata.Name} assets")
    {
        _plugin = plugin;
    }

    public override async void DoAction(Action resolve, Action<string> reject)
    {
        try
        {
            var bundlesPath = Path.Combine(_plugin.PluginFolderPath, GlobalModDefines.AssetBundlesFolder);
            if (Directory.Exists(bundlesPath))
                foreach (var file in Directory.GetFiles(bundlesPath))
                {
                    var assetBundleName = Path.GetFileNameWithoutExtension(file);
                    if (Path.GetExtension(file) != ".bundle") continue;

                    var assetBundle = AssetBundle.LoadFromFile(file);

                    if (assetBundle == null)
                    {
                        Logger.LogError(
                            $"Failed to load AssetBundle {_plugin.SpaceWarpMetadata.ModID}/{assetBundleName}");
                        continue;
                    }

                    await AssetManager.RegisterAssetBundle(_plugin.SpaceWarpMetadata.ModID, assetBundleName,
                        assetBundle);
                    Logger.LogInfo($"Loaded AssetBundle {_plugin.SpaceWarpMetadata.ModID}/{assetBundleName}");
                }
            else
                Logger.LogInfo(
                    $"Did not load asset bundles for {_plugin.SpaceWarpMetadata.Name} as no asset bundles folder existed!");

            var imagesPath = Path.Combine(_plugin.PluginFolderPath, GlobalModDefines.ImageAssetsFolder);
            if (Directory.Exists(imagesPath))
            {
                var directoryInfo = new DirectoryInfo(imagesPath);
                foreach (var file in directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                             .Select(fileInfo => fileInfo.FullName))
                {
                    //We have to make sure it uses '/' as the path separator and toLower() the names
                    var assetPathList = PathHelpers.GetRelativePath(imagesPath, file)
                        .Split(Path.DirectorySeparatorChar);
                    var assetPath = "";

                    for (var i = 0; i < assetPathList.Length; i++)
                    {
                        assetPath += assetPathList[i].ToLower();
                        if (i != assetPathList.Length - 1) assetPath += "/";
                    }

                    assetPath = $"images/{assetPath}";

                    var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false)
                    {
                        filterMode = FilterMode.Point
                    };

                    try
                    {
                        var fileData = File.ReadAllBytes(file);
                        tex.LoadImage(fileData); // Will automatically resize
                        AssetManager.RegisterSingleAsset(_plugin.SpaceWarpMetadata.ModID, assetPath, tex);
                    }
                    catch (Exception e)
                    {
                        _plugin.ModLogger.LogError(e.ToString());
                    }
                }
            }
            else
            {
                Logger.LogInfo(
                    $"Did not load images bundles for {_plugin.SpaceWarpMetadata.Name} as no images folder existed! (should be at {imagesPath}");
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