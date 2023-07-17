using System.IO;
using BepInEx.Configuration;
using JetBrains.Annotations;
using SpaceWarp.API.Mods.JSON;

namespace SpaceWarp.API.Mods;

public class SpaceWarpPluginDescriptor
{
    public SpaceWarpPluginDescriptor([CanBeNull] ISpaceWarpMod plugin, string guid, string name, ModInfo swInfo, DirectoryInfo folder, [CanBeNull] ConfigFile configFile = null)
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
    [CanBeNull] public readonly ConfigFile ConfigFile;
}