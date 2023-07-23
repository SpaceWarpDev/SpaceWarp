using System.Collections.Generic;
using System.Collections.Immutable;

namespace SpaceWarp.API.Configuration;

public class EmptyConfigFile : IConfigFile
{
    public void Save()
    {
    }

    public IConfigEntry this[string section, string key] => throw new KeyNotFoundException($"{section}/{key}");

    public IConfigEntry Bind<T>(string section, string key, T defaultValue = default, string description = "")
    {
        throw new System.NotImplementedException();
    }

    public IReadOnlyList<string> Sections => new List<string>();

    public IReadOnlyList<string> this[string section] => throw new KeyNotFoundException($"{section}");
}