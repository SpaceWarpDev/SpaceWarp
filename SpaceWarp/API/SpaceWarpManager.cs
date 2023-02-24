using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;
using SpaceWarp.API.Logging;
using Object = UnityEngine.Object;

namespace SpaceWarp.API
{
    public class SpaceWarpManager : MonoBehaviour
    {
        private BaseModLogger _modLogger;
        
        public const string MODS_FOLDER_NAME = "Mods";
        public const string SPACE_WARP_CONFIG = "space_warp_config.json";

        public GlobalConfiguration SpaceWarpConfiguration;
        
        public void Start()
        {
            string modsFolder = Application.dataPath + "/" + MODS_FOLDER_NAME;
            string configLocation = modsFolder + "/" + SPACE_WARP_CONFIG;
            if (!File.Exists(configLocation))
            {
                SpaceWarpConfiguration = new GlobalConfiguration();
                SpaceWarpConfiguration.ApplyDefaultValues();
            }
            else
            {
                SpaceWarpConfiguration = JsonConvert.DeserializeObject<GlobalConfiguration>(File.ReadAllText(configLocation));
            }

            File.WriteAllLines(configLocation,new string[] {JsonConvert.SerializeObject(SpaceWarpConfiguration)});
            _modLogger = new ModLogger("Space Warp");
            _modLogger.Info("Warping Spacetime");

            foreach (string dir in Directory.GetDirectories(modsFolder))
            {
                string modName = Path.GetFileName(dir);
                _modLogger.Info($"Found mod: {modName}, attempting to do a simple load of the mod");
                ModLogger newModLogger = new ModLogger(modName);
                // Now we load all assemblies under the code folder of the mod
                string codePath = dir + "/code/";
                if (Directory.Exists(codePath))
                {
                    List<Assembly> modAssemblies = new List<Assembly>();
                    foreach (string file in Directory.GetFiles(codePath))
                    {
                        modAssemblies.Add(Assembly.LoadFrom(file));
                    }

                    Type mainModType = null;
                    foreach (Assembly asm in modAssemblies)
                    {
                        mainModType = asm.GetTypes().FirstOrDefault(type => type.GetCustomAttributes<MainModAttribute>() != null);
                        if (mainModType != null) break;
                    }

                    if (mainModType == null)
                    {
                        _modLogger.Error($"Could not load mod: {modName}, no type with [MainMod] exists");
                        continue;
                    }

                    GameObject modObject = new GameObject($"MOD: {modName}");
                    Object.DontDestroyOnLoad(modObject);
                    Mod modComponent = (Mod)modObject.AddComponent(mainModType);
                    modObject.transform.SetParent(transform.parent);
                    modComponent.Logger = newModLogger;
                    modComponent.Manager = this;
                    modObject.SetActive(true);
                    _modLogger.Info($"Loaded: {modName}");
                    modComponent.Initialize();
                }
                else
                {
                    _modLogger.Error($"Could not load mod: {modName}, code does not exist");
                }
            }

        }

        public void Update()
        {
            // TODO: Space Warp
        }
    }
}