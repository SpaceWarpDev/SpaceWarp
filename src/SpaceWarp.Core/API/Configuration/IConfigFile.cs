using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace SpaceWarp.API.Configuration;

[PublicAPI]
public interface IConfigFile
{
    public void Save();

    public IConfigEntry this[string section, string key] { get; }

    public IConfigEntry Bind<T>(string section, string key, T defaultValue = default, string description = "");
    
    public IReadOnlyList<string> Sections { get; }
    public IReadOnlyList<string> this[string section] { get; }
}