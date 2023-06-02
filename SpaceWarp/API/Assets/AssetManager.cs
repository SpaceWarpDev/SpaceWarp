using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceWarp.API.Lua;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace SpaceWarp.API.Assets;

[SpaceWarpLuaAPI("Assets")]
public static class AssetManager
{
    private static readonly Dictionary<string, UnityObject> AllAssets = new();

    internal static async Task RegisterAssetBundle(string modId, string assetBundleName, AssetBundle assetBundle)
    {
        assetBundleName = assetBundleName.Replace(".bundle", "");
        var logger = Logger.CreateLogSource($"{modId}/{assetBundleName}");

        var names = assetBundle.GetAllAssetNames();

        foreach (var name in names)
        {
            var assetName = name;

            if (assetName.ToLower().StartsWith("assets/"))
            {
                assetName = assetName["assets/".Length..];
            }

            if (assetName.ToLower().StartsWith(assetBundleName + "/"))
            {
                assetName = assetName[(assetBundleName.Length + 1)..];
            }

            var path = modId + "/" + assetBundleName + "/" + assetName;
            path = path.ToLower();

            await assetBundle.LoadAssetAsync(name);

            var bundleObject = assetBundle.LoadAssetAsync(name).asset;
            logger.LogInfo($"registering path \"{path}\"");

            AllAssets.Add(path, bundleObject);
        }
    }

    // if (bundleObjects.Length != names.Length)
    // {
    // 	logger.Critical("bundle objects length and name lengths do not match");
    // 	logger.Info("going to dump objects and names");
    // 	logger.Info("Names");
    // 	for (int i = 0; i < names.Length; i++)
    // 	{
    // 		logger.Info($"{i} - {names[i]}");
    // 	}
    //
    // 	logger.Info("Objects");
    // 	for (int i = 0; i < bundleObjects.Length; i++)
    // 	{
    // 			logger.Info($"{i} - {bundleObjects[i]}");
    // 	}
    // 	throw new System.Exception("bundle objects length and name lengths do not match");
    // }


    internal static void RegisterSingleAsset<T>(string modId, string internalAssetPath, T asset) where T : UnityObject
    {
        var path = $"{modId}/{internalAssetPath}";
        path = path.ToLower();
        var logger = Logger.CreateLogSource($"{path}");
        logger.LogInfo($"registering path \"{path}\"");
        AllAssets.Add(path, asset);
    }

    /// <summary>
    ///     Gets an asset from the specified asset path
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    /// <param name="path">an asset path, format: {mod_id}/{asset_bundle}/{asset_path}</param>
    /// <returns></returns>
    public static T GetAsset<T>(string path) where T : UnityObject
    {
        path = path.ToLower();
        var subPaths = path.Split('/', '\\');
        if (subPaths.Length < 3)
        {
            throw new ArgumentException(
                "Invalid path, asset paths must follow to following structure: {mod_id}/{asset_bundle}/{asset_path}");
        }

        if (!AllAssets.TryGetValue(path, out var value))
        {
            throw new IndexOutOfRangeException($"Unable to find asset at path \"{path}\"");
        }

        if (value is not T tValue)
        {
            throw new InvalidCastException(
                $"The asset at path {path} isn't of type {typeof(T).Name} but of type {value.GetType().Name}");
        }

        return tValue;
    }

    /// <summary>
    ///     Tries to get an asset from the specified asset path
    /// </summary>
    /// <typeparam name="T">The type</typeparam>
    /// <param name="path">an asset path, format: {mod_id}/{asset_bundle}/{asset_name}</param>
    /// <param name="asset">the asset output</param>
    /// <returns>Whether or not the asset exists and is loaded</returns>
    public static bool TryGetAsset<T>(string path, out T asset) where T : UnityObject
    {
        path = path.ToLower();
        asset = null;
        var subPaths = path.Split('/', '\\');

        if (subPaths.Length < 3)
        {
            return false;
        }

        if (!AllAssets.TryGetValue(path, out var value))
        {
            return false;
        }

        if (value is not T tValue)
        {
            return false;
        }

        asset = tValue;

        return true;
    }
    
    /// <summary>
    ///     Gets an asset from the specified asset path
    /// </summary>
    /// <param name="path">an asset path, format: {mod_id}/{asset_bundle}/{asset_path}</param>
    /// <returns></returns>
    public static UnityObject GetAsset(string path)
    {
        path = path.ToLower();
        var subPaths = path.Split('/', '\\');
        if (subPaths.Length < 3)
        {
            throw new ArgumentException(
                "Invalid path, asset paths must follow to following structure: {mod_id}/{asset_bundle}/{asset_path}");
        }

        if (!AllAssets.TryGetValue(path, out var value))
        {
            throw new IndexOutOfRangeException($"Unable to find asset at path \"{path}\"");
        }

        return value;
    }

    /// <summary>
    ///     Tries to get an asset from the specified asset path
    /// </summary>
    /// <param name="path">an asset path, format: {mod_id}/{asset_bundle}/{asset_name}</param>
    /// <param name="asset">the asset output</param>
    /// <returns>Whether or not the asset exists and is loaded</returns>
    public static bool TryGetAsset(string path, out UnityObject asset)
    {
        path = path.ToLower();
        asset = null;
        var subPaths = path.Split('/', '\\');

        if (subPaths.Length < 3)
        {
            return false;
        }

        if (!AllAssets.TryGetValue(path, out var value))
        {
            return false;
        }

        asset = value;

        return true;
    }
}