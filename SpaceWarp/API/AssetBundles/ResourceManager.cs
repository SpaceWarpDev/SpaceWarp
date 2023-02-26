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
				string path = modId + "/" + assetBundleName + "/" + names[i];
				Object bundleObject = bundleObjects[i];

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
	}
}
