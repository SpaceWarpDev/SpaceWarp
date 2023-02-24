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
        public static void OnGameStarted()
        {
            Console.WriteLine("[Space Warp] Loaded");
            string modsFolder = Application.dataPath + "/" + SpaceWarpManager.MODS_FOLDER_NAME;
            // Create the mods directory if one does not yet exist
            Console.WriteLine("[Space Warp] ");
            Directory.CreateDirectory(modsFolder);
            GameObject spaceWarp = new GameObject("Space Warp");
            Object.DontDestroyOnLoad(spaceWarp);
            spaceWarp.AddComponent<SpaceWarpManager>();
            spaceWarp.SetActive(true);
        }
    }
}
