using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ksp2_mod_loader_patcher
{
	public static class SteamInstallLocationFinder
	{
		const string PATH_TO_LIBRARY_FILE = "/steamapps/libraryfolders.vdf";
		const string PATH_TO_KSP2_FROM_STEAM = "/steamapps/common/Kerbal Space Program 2";
		const string KSP2_GAME_APP_ID = "954850";

		public static bool TryGetInstallPath(out string installPath)
		{
			string steamLocation = GetSteamInstallLocation();
			
			// steam not installed?
			if (steamLocation == null)
			{
				installPath = null;
				return false;
			}

			string libraryFilePath = steamLocation + PATH_TO_LIBRARY_FILE;

			// unable to find library file (looking in the wrong location?) this really should never happen
			if (!File.Exists(libraryFilePath))
			{
				installPath = null;
				return false;
			}

			// unable to parse vdf file (file corrupted?)
			if (!VdfFile.TryParse(libraryFilePath, out VdfFile vdfFile))
			{
				installPath = null;
				return false;
			}

			List<VdfFile> locations = new List<VdfFile>();
			foreach(string key in vdfFile.GetChildNodes())
			{
				VdfFile child_file = vdfFile.GetChild(key);
				locations.Add(child_file);
			}

			foreach(VdfFile location in locations)
			{
				VdfFile app = location.GetChild("apps");

				// probably a corrupt file but we might as well continue
				if (app == null)
				{
					continue;
				}

				bool containsKsp2 = app.GetChildNodes().Contains(KSP2_GAME_APP_ID);
				if (containsKsp2)
				{
					string path = location["path"];

					// probably a corrupt file but we might as well continue
					if (path == null)
					{
						continue;
					}

					installPath = path + PATH_TO_KSP2_FROM_STEAM;

					if (!Directory.Exists(installPath))
					{
						return false;
					}

					return true;
				}
			}

			// looks like in some cases the vdf file wont contain the id, but the game will still be installed. Since we already have the steam install locations we might as well check if the folder is there in any of them anyway
			foreach (VdfFile location in locations)
			{
				string path = location["path"];

				// probably a corrupt file but we might as well continue
				if (path == null)
				{
					continue;
				}

				string targetPath = path + PATH_TO_KSP2_FROM_STEAM;

				if (Directory.Exists(targetPath))
				{
					installPath = targetPath;
					return true;
				}
			}

			installPath = null;
			return false;
		}
		
		static string GetSteamInstallLocation()
		{
			string registryKey;

			if (Environment.Is64BitOperatingSystem)
			{
				registryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam";
			}
			else
			{
				registryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam";
			}

			string path = Registry.GetValue(registryKey, "InstallPath", null) as string;
			if (path != null)
			{
				path = path.TrimEnd('/','\\');
			}

			return path;
		}
	}
}
