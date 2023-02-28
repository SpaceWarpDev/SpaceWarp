using System;
using System.Collections.Generic;
using System.IO;
using SpaceWarp.API.Managers;
using Newtonsoft.Json;

namespace SpaceWarp.API.Configuration;

public class ConfigurationManager : Manager
{
    private readonly Dictionary<string, (Type configType, object configObject, string path)> _modConfigurations = new Dictionary<string, (Type configType, object configObject, string path)>();
        
    public void Add(string id, (Type configType, object configObject, string path) configuration)
    {
        if (_modConfigurations.ContainsKey(id))
        {
            return;
        }

        _modConfigurations[id] = configuration;
    }

    public bool TryGet(string id, out (Type configType, object configObject, string path) config)
    {
        return _modConfigurations.TryGetValue(id, out config);
    }

    public void UpdateConfiguration(string id)
    {
        if (!_modConfigurations.TryGetValue(id, out (Type, object, string) config))
        {
            return;
        }

        // Saves the new configuration
        File.WriteAllText(config.Item3,JsonConvert.SerializeObject(config.Item2));
    }
}