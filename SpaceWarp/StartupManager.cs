using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using System.IO;
using SpaceWarp.API;
using Object = UnityEngine.Object;

namespace SpaceWarp
{
    public static class StartupManager
    {
        public static SpaceWarpManager SpaceWarpObject;
        public static bool _hasInitialized = false;

        public static void OnGameStarted()
        {
			// since OnGameStarted could be called multiple times, we want to make sure we only do anything on first call.
			if (_hasInitialized)
                return;
            _hasInitialized = true;

            Console.WriteLine("[Space Warp] Loaded");
            string modsFolder = Application.dataPath + "/" + SpaceWarpManager.MODS_FOLDER_NAME;
            // Create the mods directory if one does not yet exist
            Console.WriteLine("[Space Warp] ");
            Directory.CreateDirectory(modsFolder);
            GameObject spaceWarp = new GameObject("Space Warp");
            Object.DontDestroyOnLoad(spaceWarp);
            SpaceWarpObject = spaceWarp.AddComponent<SpaceWarpManager>();
            spaceWarp.SetActive(true);
        }
    }
}
