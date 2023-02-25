using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using KSP.Game;
using UnityEngine;
using Newtonsoft.Json;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Logging;
using SpaceWarp.Patching;

namespace SpaceWarp.API
{
    /// <summary>
    /// Handles all the SpaceWarp initialization and mod processing.
    /// </summary>
    public class SpaceWarpManager : KerbalMonoBehaviour
    {
        private BaseModLogger _modLogger;
        
        public const string MODS_FOLDER_NAME = "Mods";
        public static string MODS_FULL_PATH = Application.dataPath + "/" + MODS_FOLDER_NAME;

        public const string SPACE_WARP_CONFIG_FILE_NAME = "space_warp_config.json";
        public static string SPACEWARP_CONFIG_FULL_PATH = MODS_FULL_PATH + "/" + SPACE_WARP_CONFIG_FILE_NAME;

        public SpaceWarpGlobalConfiguration SpaceWarpConfiguration;

        internal List<Mod> allModScripts = new List<Mod>();
        internal List<(string,ModInfo)> modLoadOrder = new List<(string,ModInfo)>();
        
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

        private static List<(string, ModInfo)> _allEnabledModInfo = new List<(string, ModInfo)>();
        internal void ReadMods()
        {
            _modLogger.Info("Reading mods");
            string[] modDirectories;
            try
            {
                modDirectories = Directory.GetDirectories(MODS_FULL_PATH);
            }
            catch(Exception exception)
            {
                _modLogger.Critical($"Unable to open mod path: {MODS_FULL_PATH}\nException:{exception}");
                return;
            }

            if (modDirectories.Length == 0)
            {
                _modLogger.Warn("No mods were found! No panic though.");
            }

            foreach (string modFolderuntrimmedU in modDirectories)
            {
                string modFolder = modFolderuntrimmedU.TrimEnd('/', '\\');

                string modName = Path.GetFileName(modFolder);
                if (!File.Exists(modFolder + "\\modinfo.json"))
                {
                    _modLogger.Warn($"Found mod {Path.GetFileName(modFolder)} without modinfo.json");
                    continue;
                }

                if (File.Exists(modFolder + "\\.ignore"))
                {
                    _modLogger.Info($"Skipping mod {Path.GetFileName(modFolder)} due to .ignore file");
                    continue;
                }
                _modLogger.Info($"Found mod: {Path.GetDirectoryName(modFolder)}, adding to enable mods");

                ModInfo info = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(modFolder + "\\modinfo.json"));
                string name = Path.GetFileName(modFolder);
                _allEnabledModInfo.Add((name,info));
            }
            ResolveLoadOrder();
        }

        private bool isAboveVersion(string toCheck, string ver)
        {
            if (ver == "")
            {
                return true;
            }

            string[] sem_ver = toCheck.Split('.');
            string[] needed_ver = ver.Split('.');
            for (int i = 0; i < needed_ver.Length; i++)
            {
                if (needed_ver[i] == "*")
                {
                    continue;
                }

                if (Int32.Parse(sem_ver[i]) < Int32.Parse(needed_ver[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool isBelowVersion(string toCheck, string ver)
        {
            if (ver == "")
            {
                return true;
            }

            string[] sem_ver = toCheck.Split('.');
            string[] needed_ver = ver.Split('.');
            for (int i = 0; i < needed_ver.Length; i++)
            {
                if (needed_ver[i] == "*")
                {
                    continue;
                }

                if (Int32.Parse(sem_ver[i]) > Int32.Parse(needed_ver[i]))
                {
                    return false;
                }
            }

            return true;
        }
        private bool DependenciesResolved(ModInfo mod)
        {
            foreach (var dependency in mod.dependencies)
            {
                _modLogger.Info($"{mod.name} dependency - {dependency.id} {dependency.version.min}-{dependency.version.max}");
                var depName = dependency.id;
                var depVersion = dependency.version;
                bool found = false;
                foreach (var loaded in modLoadOrder)
                {
                    if (loaded.Item2.mod_id == depName)
                    {
                        var depLoadedVersion = loaded.Item2.version;
                        if (!isAboveVersion(depLoadedVersion, depVersion.min)) return false;
                        if (!isBelowVersion(depLoadedVersion, depVersion.max)) return false;
                        found = true;
                    }
                }

                if (!found) return false;
            }

            return true;
        }
        private void ResolveLoadOrder()
        {
            //TODO: Make this way more optimized!
            _modLogger.Info("Resolving Load Order");
            bool changed = true;
            while (changed)
            {
                changed = false;
                List<int> toRemove = new List<int>();
                for (int i = 0; i < _allEnabledModInfo.Count; i++)
                {
                    _modLogger.Info("Attempting to resolve dependencies for " + _allEnabledModInfo[i].Item1);
                    if (DependenciesResolved(_allEnabledModInfo[i].Item2))
                    {
                        modLoadOrder.Add(_allEnabledModInfo[i]);
                        toRemove.Add(i);
                        changed = true;
                    }
                }

                for (int i = toRemove.Count - 1; i >= 0; i--)
                {
                    _allEnabledModInfo.RemoveAt(toRemove[i]);
                }
            }

            if (_allEnabledModInfo.Count > 0)
            {
                foreach ((string modName, ModInfo info) in _allEnabledModInfo)
                {
                    _modLogger.Warn($"Skipping loading of {modName} as not all dependencies could be met");
                }
            }
        }

        /// <summary>
        /// Runs the mod initialization procedures.
        /// </summary>
        internal void InitializeMods()
        {
            _modLogger.Info("Initializing mods");
            

            foreach ((string modName, ModInfo info) in modLoadOrder)
            {
                string modFolder = MODS_FULL_PATH + "/" + modName;

                _modLogger.Info($"Found mod: {modName}, attempting to load mod");

                // Now we load all assemblies under the code folder of the mod
                string codePath = modFolder + GlobalModDefines.BINARIES_FOLDER;

                if (Directory.Exists(codePath))
                {
                    if (!TryLoadMod(codePath, modName, out Type mainModType))
                    {
						// error logging is done inside TryLoadMod
						continue;
                    }

                    InitializeModObject(modName, info, mainModType);
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
            string[] files;
            try
            {
                files = Directory.GetFiles(codePath);
			}
            catch
            {
                _modLogger.Error("Could not load mod: {modName}, unable to read directory");
                mainModType = null;
				return false;
            }

            List<Assembly> modAssemblies = new List<Assembly>();
            foreach (string file in files)
            {
                // we only want to load dll files, ignore everything else
                if (!file.EndsWith(".dll"))
                {
                    _modLogger.Warn($"Non-dll file found in \"{codePath}\" \"{file}\", Ignoring");
					continue;
				}

                Assembly asm;
                try
                {
					asm = Assembly.LoadFrom(file);
				}
                catch(Exception exeption)
                {
                    _modLogger.Error($"Could not load mod: {modName}, Failed to load assembly {file}\nException: {exeption}");
                    mainModType = null;

                    return false;
                }

				modAssemblies.Add(asm);
            }

            mainModType = null;
            foreach (Assembly asm in modAssemblies)
            {
                mainModType = asm.GetTypes().FirstOrDefault(type => type.GetCustomAttribute<MainModAttribute>() != null);
                if (mainModType != null) break;
            }

            if (mainModType == null)
            {
				_modLogger.Error($"Could not load mod: {modName}, no type with [MainMod] exists");
				return false;
            }

            if (!typeof(Mod).IsAssignableFrom(mainModType))
            {
                _modLogger.Error($"Could not load mod: {modName}, the found class ({mainModType.FullName}) with [MainMod] doesn't inherit from {nameof(Mod)}");
                mainModType = null;
				return false;
            }

            return true;

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
                try
                {
                    string json = File.ReadAllText(SPACEWARP_CONFIG_FULL_PATH);
                    SpaceWarpConfiguration = JsonConvert.DeserializeObject<SpaceWarpGlobalConfiguration>(json);
                }
                catch (Exception exception)
                {
                    //TODO: log this in a nicer way, for now I guess we can just construct a new logger
                    new ModLogger("Space Warp").Error($"Loading space warp config failed\nException: {exception}");

                    File.Delete(SPACEWARP_CONFIG_FULL_PATH);
                    InitializeSpaceWarpConfig();
                    return;
                }
            }
            
            try
            {
				File.WriteAllLines(SPACEWARP_CONFIG_FULL_PATH, new[] { JsonConvert.SerializeObject(SpaceWarpConfiguration) });
			}
            catch(Exception exception)
            {
                //TODO: log this in a nicer way, for now I guess we can just construct a new logger
                new ModLogger("Space Warp").Error($"Saving the spacewarp config failed\nException: {exception}");
			}
        }

        /// <summary>
        /// Initializes a mod object.
        /// </summary>
        /// <param name="modName">The mod name to initialize.</param>
        /// <param name="mainModType">The mod type to initialize.</param>
        /// <param name="newModLogger">The new mod logger to spawn</param>
        private void InitializeModObject(string modName, ModInfo info, Type mainModType)
        {
            ModLogger newModLogger = new ModLogger(info.name);

            GameObject modObject = new GameObject($"Mod: {modName}");
            Mod modComponent = (Mod)modObject.AddComponent(mainModType);
            allModScripts.Add(modComponent);
            modObject.transform.SetParent(transform.parent);
            modComponent.Logger = newModLogger;
            modComponent.Manager = this;
            modComponent.Info = info;

            modObject.SetActive(true);
            _modLogger.Info($"Loaded: {modName}");

			// we probably dont want to completley stop loading mods if 1 mod throws an exception on Initialize
			try
			{
				modComponent.Initialize();
			}
            catch(Exception exception)
            {
                _modLogger.Critical($"Exception in {modName} Initialize(): {exception}");
            }
        }

        internal void AfterInitializationTasks()
        {
            foreach (Mod mod in allModScripts)
            {
                try
                {
					mod.AfterInitialization();
				}
                catch(Exception exception)
                {
					_modLogger.Critical($"Exception in {mod.name} AfterInitialization(): {exception}");
				}
            }
        }
    }
}