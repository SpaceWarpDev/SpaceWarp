using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpaceWarp.API.AssetBundles
{
	public static class AssetManager
	{
		static Dictionary<string, UnityEngine.Object> _allAssets = new Dictionary<string, UnityEngine.Object>();

		internal static void RegisterAssetBundle(string modId, string assetBundleName, AssetBundle assetBundle)
		{
			// TODO: use async loading instead?

			Object[] bundleObjects = assetBundle.LoadAllAssets();
			string[] names = assetBundle.GetAllAssetNames();

			if (bundleObjects.Length != names.Length)
			{
				throw new System.Exception("bundle objects length and name lengths do not match");
			}

			for(int i = 0; i < bundleObjects.Length; i++)
			{
				List<string> assetNamePath = names[i].Split('/').ToList();
				if (assetNamePath[0] == "Assets")
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
				Object bundleObject = bundleObjects[i];

				System.Console.WriteLine($"registering path \"{path}\"");

				_allAssets.Add(path, bundleObject);
			}
		}

		/// <summary>
		/// Gets an asset from the specified asset path
		/// </summary>
		/// <typeparam name="T">The type</typeparam>
		/// <param name="path">an asset path, format: {mod_id}/{asset_bundle}/{asset_path}</param>
		/// <returns></returns>
		public static T GetAsset<T>(string path) where T: UnityEngine.Object
		{
			string[] subPaths = path.Split('/', '\\');
			if (subPaths.Length < 3)
			{
				throw new System.ArgumentException("Invalid path, asset paths must follow to following structure: {mod_id}/{asset_bundle}/{asset_path}");
			}

			if (!_allAssets.TryGetValue(path, out Object value))
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
		public static bool TryGetAsset<T>(string path, out T asset) where T : UnityEngine.Object
		{
			asset = null;
			string[] subPaths = path.Split('/', '\\');
			if (subPaths.Length < 3)
			{
				return false;
			}
			if (!_allAssets.TryGetValue(path, out Object value))
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
}
