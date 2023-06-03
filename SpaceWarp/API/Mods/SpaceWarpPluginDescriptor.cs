using System.IO;
using JetBrains.Annotations;
using SpaceWarp.API.Mods.JSON;

namespace SpaceWarp.API.Mods;

public class SpaceWarpPluginDescriptor
{
    public SpaceWarpPluginDescriptor([CanBeNull] BaseSpaceWarpPlugin plugin, string guid, ModInfo swInfo, DirectoryInfo folder)
    {
        Plugin = plugin;
        Guid = guid;
        SWInfo = swInfo;
        Folder = folder;
    }

    [CanBeNull] public BaseSpaceWarpPlugin Plugin;
    public string Guid;
    public ModInfo SWInfo;
    public DirectoryInfo Folder;
}