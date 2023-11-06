using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using JetBrains.Annotations;

namespace SpaceWarp.API.Configuration;

[PublicAPI]
public class BepInExConfigFile : IConfigFile
{

    public readonly ConfigFile AdaptedConfigFile;

    public BepInExConfigFile(ConfigFile adaptedConfigFile)
    {
        AdaptedConfigFile = adaptedConfigFile;
    }

    public void Save()
    {
        AdaptedConfigFile.Save();
    }

    public IConfigEntry this[string section, string key] => new BepInExConfigEntry(AdaptedConfigFile[section, key]);

    public IConfigEntry Bind<T>(string section, string key, T defaultValue = default, string description = "")
    {
        return new BepInExConfigEntry(AdaptedConfigFile.Bind(section, key, defaultValue, description));
    }

    public IReadOnlyList<string> Sections => AdaptedConfigFile.Keys.Select(x => x.Section).Distinct().ToList();

    public IReadOnlyList<string> this[string section] => AdaptedConfigFile.Keys.Where(x => x.Section == section)
        .Select(x => x.Key).ToList();
}