using System;
using UnityEngine;
using System.IO;
using SpaceWarp.API;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;
using HarmonyLib;
using System.Reflection;
using SpaceWarp.Patching;
using KSP.Logging;
using static UnityEngine.Application;

namespace Doorstop {

    public class Entrypoint {
        /// <summary>
        /// EntryPoint for Spacewarp, called from Doorstop
        /// </summary>
        public static void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            var LogListener = new SpaceWarp.UI.SpaceWarpConsoleLogListener();
            KspLogManager.AddLogCallback(LogListener.LogCallback);
        }

        static bool patched = false;

        /// <summary>
        /// Add OnGameStarted as postfix to StartGame
        /// </summary>
        static void OnSceneLoaded(Scene unused1, LoadSceneMode unused2)
        {
            if (!patched) {
                InitializePatches();
                patched = true;
            }
        }

		/// <summary>
		/// Initializes Harmony
		/// </summary>

		static void InitializePatches()
		{
			Harmony harmony = new Harmony("com.github.x606.spacewarp");

			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}

namespace SpaceWarp
{

    /// <summary>
    /// Starts the SpaceWarm mod manager
    /// </summary>
    public static class StartupManager
    {
        public static SpaceWarpManager SpaceWarpObject;
        public static bool _hasInitialized = false;

        /// <summary>
        /// This will be called once the KSP2 game is loaded.
        /// </summary>
        /// <returns></returns>
        public static void OnGameStarted()
        {
            // since OnGameStarted could be called multiple times, we want to make sure we only do anything on first call.
            if (_hasInitialized)
            {
                return;
            }

            CreateModDirectoryIfNotExists();
            CreateSpaceWarpManager();

            Console.WriteLine("[Space Warp] Space Warp mod manager loaded!");
            _hasInitialized = true;
        }

        /// <summary>
        /// Creates the space warp manager object.
        /// </summary>
        private static void CreateSpaceWarpManager()
        {
            GameObject spaceWarp = new GameObject("Space Warp");
            Object.DontDestroyOnLoad(spaceWarp);
            SpaceWarpObject = spaceWarp.AddComponent<SpaceWarpManager>();
            spaceWarp.SetActive(true);
            // SpaceWarpObject.Initialize();
        }

        /// <summary>
        /// Creates the mod folder if it doesn't exist.
        /// </summary>
        private static void CreateModDirectoryIfNotExists()
        {
            Directory.CreateDirectory(SpaceWarpManager.MODS_FULL_PATH);
        }
    }
}
