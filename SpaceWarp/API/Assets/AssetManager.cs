using System;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace SpaceWarp.API.Assets;

public static class AssetManager
{
	private static readonly Dictionary<string, UnityObject> AllAssets = new();

	internal static void RegisterAssetBundle(string modId, string assetBundleName, AssetBundle assetBundle)
	{
		assetBundleName = assetBundleName.Replace(".bundle", "");
		ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource($"{modId}/{assetBundleName}");
		// TODO: use async loading instead?

		// Object[] bundleObjects = assetBundle.LoadAllAssets();
		string[] names = assetBundle.GetAllAssetNames();

		for (int i = 0; i < names.Length; i++)
		{
			var assetName = names[i];
			if (assetName.ToLower().StartsWith("assets/"))
				assetName = assetName["assets/".Length..];
			if (assetName.ToLower().StartsWith(assetBundleName + "/"))
				assetName = assetName[(assetBundleName.Length + 1)..];
				
			string path = modId + "/" + assetBundleName + "/" + assetName;
			path = path.ToLower();
			var bundleObject = assetBundle.LoadAsset(names[i]);

			logger.LogInfo($"registering path \"{path}\"");

			AllAssets.Add(path, bundleObject);
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
	}

	internal static void RegisterSingleAsset<T>(string modId, string internalAssetPath, T asset) where T : UnityObject
	{
		var path = $"{modId}/{internalAssetPath}";
		path = path.ToLower();
		ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource($"{path}");
		logger.LogInfo($"registering path \"{path}\"");
		AllAssets.Add(path,asset);
	}

	private static ManualLogSource PatchLogger = BepInEx.Logging.Logger.CreateLogSource($"Asset Patching");
	
	/// <summary>
	/// Attempt to patch an asset at a path
	/// </summary>
	/// <param name="assetPath">The path to the asset to patch</param>
	/// <param name="asset">The asset to patch with</param>
	/// <typeparam name="T">The type of the asset that the asset is being patched with</typeparam>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the asset does not exist</exception>
	/// <exception cref="ArgumentException">Thrown if the type of the patching asset cannot be assigned to the type of the patched asset</exception>
	public static void PatchAsset<T>(string assetPath, T asset) where T : UnityObject
	{
		if (!AllAssets.Contains<>(assetPath))
		{
			throw new ArgumentOutOfRangeException(nameof(assetPath));
		}

		if (AllAssets[assetPath].GetType().IsAssignableFrom(typeof(T)))
		{
			PatchLogger.LogError($"Attempting to patch {assetPath} which is a(n) {AllAssets[assetPath].GetType().FullName} with an object that is {typeof(T).FullName}. This is an error.");
			throw new ArgumentException(nameof(asset));
		}

		AllAssets[assetPath] = asset;
	}

	/// <summary>
	/// Gets an asset from the specified asset path
	/// </summary>
	/// <typeparam name="T">The type</typeparam>
	/// <param name="path">an asset path, format: {mod_id}/{asset_bundle}/{asset_path}</param>
	/// <returns></returns>
	public static T GetAsset<T>(string path) where T: UnityEngine.Object
	{
		path = path.ToLower();
		string[] subPaths = path.Split('/', '\\');
		if (subPaths.Length < 3)
		{
			throw new ArgumentException("Invalid path, asset paths must follow to following structure: {mod_id}/{asset_bundle}/{asset_path}");
		}

		if (!AllAssets.TryGetValue(path, out UnityObject value))
		{
			throw new IndexOutOfRangeException($"Unable to find asset at path \"{path}\"");
		}

		if (value is not T tValue)
		{
			throw new InvalidCastException($"The asset at path {path} isn't of type {typeof(T).Name} but of type {value.GetType().Name}");
		}

		return tValue;
	}

	/// <summary>
	/// Tries to get an asset from the specified asset path
	/// </summary>
	/// <typeparam name="T">The type</typeparam>
	/// <param name="path">an asset path, format: {mod_id}/{asset_bundle}/{asset_name}</param>
	/// <param name="asset">the asset output</param>
	/// <returns>Whether or not the asset exists and is loaded</returns>
	public static bool TryGetAsset<T>(string path, out T asset) where T : UnityObject
	{
		path = path.ToLower();
		asset = null;
		string[] subPaths = path.Split('/', '\\');
		if (subPaths.Length < 3)
		{
			return false;
		}
		if (!AllAssets.TryGetValue(path, out UnityObject value))
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
}