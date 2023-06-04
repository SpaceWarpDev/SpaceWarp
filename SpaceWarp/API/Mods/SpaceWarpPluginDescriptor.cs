using System.IO;
using JetBrains.Annotations;
using SpaceWarp.API.Mods.JSON;

namespace SpaceWarp.API.Mods;

public class SpaceWarpPluginDescriptor
{
    public SpaceWarpPluginDescriptor([CanBeNull] BaseSpaceWarpPlugin plugin, string guid, string name, ModInfo swInfo, DirectoryInfo folder)
    {
        Plugin = plugin;
        Guid = guid;
        Name = name;
        SWInfo = swInfo;
        Folder = folder;
    }

    [CanBeNull] public BaseSpaceWarpPlugin Plugin;
    public string Guid;
    public string Name;
    public ModInfo SWInfo;
    public DirectoryInfo Folder;
}