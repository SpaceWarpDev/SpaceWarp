using System.IO;
using BepInEx.Configuration;
using JetBrains.Annotations;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Mods.JSON;

namespace SpaceWarp.API.Mods;

public class SpaceWarpPluginDescriptor
{
    public SpaceWarpPluginDescriptor([CanBeNull] ISpaceWarpMod plugin, string guid, string name, ModInfo swInfo, DirectoryInfo folder,  bool doLoadingActions = true, [CanBeNull] IConfigFile configFile = null)
    {
        Plugin = plugin;
        Guid = guid;
        Name = name;
        SWInfo = swInfo;
        Folder = folder;
        ConfigFile = configFile;
    }

    [CanBeNull] public ISpaceWarpMod Plugin;
    public readonly string Guid;
    public readonly string Name;
    public readonly ModInfo SWInfo;
    public readonly DirectoryInfo Folder;
    public bool DoLoadingActions = false;
    [CanBeNull] public IConfigFile ConfigFile;


    // Set by the version checking system
    public bool Outdated = false;
    public bool Errored = false;

    public bool Unsupported = false;
    // Used to check for mods that have not been pre-initialized
    public bool LatePreInitialize = false;
}