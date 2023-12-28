using JetBrains.Annotations;
using SpaceWarp.API.Configuration;

namespace SpaceWarp.UI.API.UI.Settings;

[PublicAPI]
public static class SettingsMenu
{
    /// <summary>
    /// Contains a list of all the registered config files
    /// </summary>
    public static Dictionary<string, IConfigFile> RegisteredConfigFiles = new();

    /// <summary>
    /// Register a manually created config file for the settings menu
    /// </summary>
    /// <param name="section">The section name for the config file</param>
    /// <param name="file">The file itself</param>
    public static void RegisterConfigFile(string section, IConfigFile file)
    {
        RegisteredConfigFiles[section] = file;
    }
}