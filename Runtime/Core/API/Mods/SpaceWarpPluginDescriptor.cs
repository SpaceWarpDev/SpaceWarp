using System.IO;
using JetBrains.Annotations;
using ReduxLib.Configuration;
using SpaceWarp.API.Mods.JSON;

namespace SpaceWarp.API.Mods;

/// <summary>
/// A descriptor for a SpaceWarp plugin.
/// </summary>
[PublicAPI]
public class SpaceWarpPluginDescriptor
{
    /// <summary>
    /// Creates a new plugin descriptor.
    /// </summary>
    /// <param name="plugin">The plugin instance.</param>
    /// <param name="guid">The plugin's GUID.</param>
    /// <param name="name">The plugin's name.</param>
    /// <param name="swInfo">The plugin's swinfo.</param>
    /// <param name="folder">The plugin's folder.</param>
    /// <param name="doLoadingActions">Whether or not to do loading actions.</param>
    /// <param name="configFile">The plugin's config file.</param>
    public SpaceWarpPluginDescriptor(
        ISpaceWarpMod? plugin,
        string guid,
        string name,
        ModInfo swInfo,
        DirectoryInfo folder,
        bool doLoadingActions = true,
        IConfigFile? configFile = null
    )
    {
        Plugin = plugin;
        Guid = guid;
        Name = name;
        SWInfo = swInfo;
        Folder = folder;
        DoLoadingActions = doLoadingActions;
        ConfigFile = configFile;
    }

    /// <summary>
    /// The plugin instance.
    /// </summary>
    public ISpaceWarpMod? Plugin;

    /// <summary>
    /// The plugin's GUID.
    /// </summary>
    public readonly string Guid;

    /// <summary>
    /// The plugin's name.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// The plugin's swinfo.
    /// </summary>
    public readonly ModInfo SWInfo;

    /// <summary>
    /// The plugin's folder.
    /// </summary>
    public readonly DirectoryInfo Folder;

    /// <summary>
    /// Whether or not to do loading actions.
    /// </summary>
    public bool DoLoadingActions;

    /// <summary>
    /// The plugin's config file.
    /// </summary>
    public IConfigFile? ConfigFile;

    /// <summary>
    /// Whether or not the plugin is outdated. Set by the version checking system.
    /// </summary>
    public bool Outdated;

    /// <summary>
    /// Whether or not the plugin is unsupported.
    /// </summary>
    public bool Unsupported;

    /// <summary>
    /// Whether or not the plugin has been pre-initialized yet.
    /// </summary>
    public bool LatePreInitialize;

    /// <summary>
    /// Is this a core mod (as in which part of the mod list does it go into)
    /// </summary>
    public bool IsCore;
}