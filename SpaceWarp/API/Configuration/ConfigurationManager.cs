using System;
using System.Collections.Generic;
using System.IO;
using SpaceWarp.API.Managers;
using Newtonsoft.Json;

namespace SpaceWarp.API.Configuration
{
    public class ConfigurationManager : Manager
    {
        public readonly Dictionary<string, (Type configType, object configObject, string path)> ModConfigurations = new Dictionary<string, (Type configType, object configObject, string path)>();
        
        public void Add(string id, (Type configType, object configObject, string path) configuration)
        {
            if (ModConfigurations.ContainsKey(id))
            {
                return;
            }

            ModConfigurations[id] = configuration;
        }

        public bool TryGet(string id, out (Type configType, object configObject, string path) config)
        {
            return ModConfigurations.TryGetValue(id, out config);
        }

        public void UpdateConfiguration(string id)
        {
            if (!ModConfigurations.TryGetValue(id, out (Type, object, string) config))
            {
                return;
            }

            // Saves the new configuration
            File.WriteAllText(config.Item3,JsonConvert.SerializeObject(config.Item2));
        }
    }
}