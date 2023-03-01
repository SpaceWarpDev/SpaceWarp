using System.Collections.Generic;
using System.Linq;
using SpaceWarp.API.Logging;
using UnityEngine;

namespace SpaceWarp.API.AssetBundles;

public static class ResourceManager
{
	static readonly Dictionary<string, Object> AllAssets = new Dictionary<string, Object>();

	internal static void RegisterAssetBundle(string modId, string assetBundleName, AssetBundle assetBundle)
	{
		assetBundleName = assetBundleName.Replace(".bundle", "");
		ModLogger logger = new ModLogger($"{modId}/{assetBundleName}");
		// TODO: use async loading instead?

		// Object[] bundleObjects = assetBundle.LoadAllAssets();
		string[] names = assetBundle.GetAllAssetNames();

		for (int i = 0; i < names.Length; i++)
		{
			List<string> assetNamePath = names[i].Split('/').ToList();
			if (assetNamePath[0].ToLower() == "assets")
			{
				assetNamePath.RemoveAt(0);
			}

			string assetName = "";
			for (int j = 0; j < assetNamePath.Count; j++)
			{
				assetName += assetNamePath[j];
				if (j != assetNamePath.Count - 1)
				{
					assetName += "/";
				}
			}
				
				
			string path = modId + "/" + assetBundleName + "/" + assetName;
			path = path.ToLower();
			Object bundleObject = assetBundle.LoadAsset(names[i]);

			logger.Info($"registering path \"{path}\"");

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
			throw new System.ArgumentException("Invalid path, asset paths must follow to following structure: {mod_id}/{asset_bundle}/{asset_path}");
		}

		if (!AllAssets.TryGetValue(path, out Object value))
		{
			throw new System.Exception($"Unable to find asset at path \"{path}\"");
		}

		if (!(value is T tValue))
		{
			throw new System.Exception($"The asset at path {path} isn't of type {typeof(T).Name} but of type {value.GetType().Name}");
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
	public static bool TryGetAsset<T>(string path, out T asset) where T : Object
	{
		path = path.ToLower();
		asset = null;
		string[] subPaths = path.Split('/', '\\');
		if (subPaths.Length < 3)
		{
			return false;
		}
		if (!AllAssets.TryGetValue(path, out Object value))
		{
			return false;
		}
		if (!(value is T tValue))
		{
			return false;
		}

		asset = tValue;

		return true;
	}
}