using System;
using UnityEngine;
using Newtonsoft.Json;
using SpaceWarp.API.Logging;

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
            _modLogger = new ModLogger("Space Warp");
            _modLogger.Info("Warping Spacetime");
        }

        public void Update()
        {
            // TODO: Space Warp
        }
    }
}