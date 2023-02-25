using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Newtonsoft.Json;
using SpaceWarp.API.Logging;
using SpaceWarp.Patching;

namespace SpaceWarp.API
{
    /// <summary>
    /// Handles all the SpaceWarp initialization and mod processing.
    /// </summary>
    public class SpaceWarpManager : MonoBehaviour
    {
        private BaseModLogger _modLogger;
        
        public const string MODS_FOLDER_NAME = "Mods";
        public static string MODS_FULL_PATH = Application.dataPath + "/" + MODS_FOLDER_NAME;

        public const string SPACE_WARP_CONFIG_FILE_NAME = "space_warp_config.json";
        public static string SPACEWARP_CONFIG_FULL_PATH = MODS_FULL_PATH + "/" + SPACE_WARP_CONFIG_FILE_NAME;

        public SpaceWarpGlobalConfiguration SpaceWarpConfiguration;

        internal List<Mod> allModScripts = new List<Mod>();
        
        public void Start()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes the SpaceWarp manager.
        /// </summary>
        private void Initialize()
        {
            InitializeSpaceWarpConfig();

            InitializeModLogger();
            
            InitializePatches();
        }

        /// <summary>
        /// Initializes Harmony
        /// </summary>

        private void InitializePatches()
        {
            LoadingScreenPatcher.AddScreens(KSP.Game.GameManager.Instance,this);
        }
        
        /// <summary>
        /// Initializes the SpaceWarp mod logger.
        /// </summary>
        private void InitializeModLogger()
        {
            _modLogger = new ModLogger("Space Warp");
            _modLogger.Info("Warping Spacetime");
        }

        /// <summary>
        /// Runs the mod initialization procedures.
        /// </summary>
        internal void InitializeMods()
        {
            _modLogger.Info("Initializing mods");
            string[] modDirectories = Directory.GetDirectories(MODS_FULL_PATH);

            if (modDirectories.Length == 0)
            {
                _modLogger.Warn("No mods were found! No panic though.");
            }

            foreach (string modFolder in modDirectories)
            {
                string modName = Path.GetFileName(modFolder);

                _modLogger.Info($"Found mod: {modName}, attempting to do a simple load of the mod");

                // Now we load all assemblies under the code folder of the mod
                string codePath = modFolder + GlobalModDefines.BINARIES_FOLDER;

                if (Directory.Exists(codePath))
                {
                    if (!TryLoadMod(codePath, modName, out Type mainModType))
                    {
                        _modLogger.Error($"Could not load mod: {modName}, code does not exist at location {codePath}");

                        continue;
                    }

                    InitializeModObject(modName, mainModType);
                }
                else
                {
                    _modLogger.Error($"Directory not found: {codePath}");
                }
            }
        }

        /// <summary>
        /// Tries to load a mod at a path
        /// </summary>
        /// <param name="codePath">The full path to this mod binaries.</param>
        /// <param name="modName">The mod name</param>
        /// <param name="mainModType">The Mod type found</param>
        /// <returns>If the mod was successfully found.</returns>
        private bool TryLoadMod(string codePath, string modName, out Type mainModType)
        {
            List<Assembly> modAssemblies = new List<Assembly>();
            foreach (string file in Directory.GetFiles(codePath))
            {
                modAssemblies.Add(Assembly.LoadFrom(file));
            }

            mainModType = null;
            foreach (Assembly asm in modAssemblies)
            {
                mainModType = asm.GetTypes().FirstOrDefault(type => type.GetCustomAttribute<MainModAttribute>() != null);
                if (mainModType != null) break;
            }

            if (mainModType != null) return true;
            _modLogger.Error($"Could not load mod: {modName}, no type with [MainMod] exists");
            return false;

        }

        /// <summary>
        /// Tried to find the SpaceWarp config file in the game, if none is round one is created.
        /// </summary>
        /// <param name="spaceWarpGlobalConfiguration"></param>
        private void InitializeSpaceWarpConfig()
        {
            if (!File.Exists(SPACEWARP_CONFIG_FULL_PATH))
            {
                SpaceWarpConfiguration = new SpaceWarpGlobalConfiguration();
                SpaceWarpConfiguration.ApplyDefaultValues();
            }
            else
            {
                SpaceWarpConfiguration = JsonConvert.DeserializeObject<SpaceWarpGlobalConfiguration>(File.ReadAllText(SPACEWARP_CONFIG_FULL_PATH));
            }

            File.WriteAllLines(SPACEWARP_CONFIG_FULL_PATH,new[] {JsonConvert.SerializeObject(SpaceWarpConfiguration)});
        }

        /// <summary>
        /// Initializes a mod object.
        /// </summary>
        /// <param name="modName">The mod name to initialize.</param>
        /// <param name="mainModType">The mod type to initialize.</param>
        /// <param name="newModLogger">The new mod logger to spawn</param>
        private void InitializeModObject(string modName, Type mainModType)
        {
            ModLogger newModLogger = new ModLogger(modName);

            GameObject modObject = new GameObject($"Mod: {modName}");
            _modLogger.Info("Before AddComponent");
            _modLogger.Info(mainModType.FullName);
            Mod modComponent = (Mod)modObject.AddComponent(mainModType);
            _modLogger.Info("After AddComponent");
            allModScripts.Add(modComponent);
            modObject.transform.SetParent(transform.parent);
            modComponent.Logger = newModLogger;
            modComponent.Manager = this;

            modObject.SetActive(true);
            _modLogger.Info($"Loaded: {modName}");

            modComponent.Initialize();
        }

        internal void AfterInitializationTasks()
        {
            foreach (Mod mod in allModScripts)
            {
                mod.AfterInitialization();
            }
        }
    }
}