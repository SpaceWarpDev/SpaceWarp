using System.IO;
using JetBrains.Annotations;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Mods.JSON;

namespace SpaceWarp.API.Mods;

[PublicAPI]
public class SpaceWarpPluginDescriptor
{
    public SpaceWarpPluginDescriptor(
        [CanBeNull] ISpaceWarpMod plugin,
        string guid,
        string name,
        ModInfo swInfo,
        DirectoryInfo folder,
        bool doLoadingActions = true,
        [CanBeNull] IConfigFile configFile = null
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

    [CanBeNull] public ISpaceWarpMod Plugin;
    public readonly string Guid;
    public readonly string Name;
    public readonly ModInfo SWInfo;
    public readonly DirectoryInfo Folder;
    public bool DoLoadingActions;
    [CanBeNull] public IConfigFile ConfigFile;

    // Set by the version checking system
    public bool Outdated;

    public bool Unsupported = false;

    // Used to check for mods that have not been pre-initialized
    public bool LatePreInitialize;
}