using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using System.IO;

namespace SpaceWarp
{
    public static class StartupManager
    {
        public const string MODS_FOLDER_NAME = "Mods";

        public static void OnGameStarted()
        {
            Console.WriteLine("[Space Warp] Loaded");
            string modsFolder = Application.dataPath + "/" + MODS_FOLDER_NAME;
            // Create the mods directory if one does not yet exist
            Console.WriteLine("[Space Warp] ");
            Directory.CreateDirectory(modsFolder);
        }
    }
}
