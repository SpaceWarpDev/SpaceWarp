using System.Linq;
using MoonSharp.Interpreter;
using ReduxLib.Configuration;

namespace SpaceWarp2.API.Config;

/// <summary>
/// A read/write view over a config file - a mod's own, or another mod's via <c>Config:Mod(id)</c>. Reaches
/// existing entries with <see cref="Get" />; it cannot define new ones (defining is an own-config operation).
/// </summary>
[MoonSharpUserData]
public sealed class ConfigView
{
    private readonly IConfigFile _file;

    internal ConfigView(IConfigFile file)
    {
        _file = file;
    }

    /// <summary>
    /// Returns a live handle over an existing entry, or <c>nil</c> if the section or entry does not exist (the
    /// owning mod may not have bound it yet).
    /// </summary>
    /// <param name="section">The config section.</param>
    /// <param name="name">The entry name within the section.</param>
    /// <returns>A handle over the entry, or null.</returns>
    public ConfigHandle Get(string section, string name) => GetFrom(_file, section, name);

    internal static ConfigHandle GetFrom(IConfigFile file, string section, string name)
    {
        if (!file.Sections.TryGet(section, out var configSection))
        {
            return null;
        }

        if (!configSection.Keys.Contains(name))
        {
            return null;
        }

        var entry = configSection[name];
        return new ConfigHandle(entry, ConfigTypes.Resolve(entry.ValueType));
    }
}
