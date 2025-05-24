using System;
using System.Collections.Generic;
using System.IO;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;

namespace SpaceWarp.API.Backend.Modding;

public interface IInternalModRegister
{
    public static IInternalModRegister Instance;
    public IEnumerable<SpaceWarpPluginDescriptor> InternalPluginDescriptors { get; }

    static DirectoryInfo GetFolder(string guid)
    {
        var path = $"./Redux/{guid}";
        var info = new DirectoryInfo(path);
        if (!info.Exists) info.Create();
        return info;
    }
    
    static SpaceWarpPluginDescriptor GetPluginDescriptorForInternalMod(Type type, string name, string guid,
        string version, string description, string source)
    {
        var plugin = new UnloadedMod(type);
        var descriptor = new SpaceWarpPluginDescriptor(plugin, guid, name, new ModInfo
        {
            Spec = new SpecVersion(2,0),
            Description = description,
            ModID = guid,
            Name = name,
            Author = "Redux Team",
            Version = version,
            Source = source,
            Dependencies = new List<DependencyInfo>()
            {
                new DependencyInfo
                {
                    ID = SpaceWarpPlugin.SpaceWarpModInfo.ModID,
                    Version = new SupportedVersionsInfo
                    {
                        Min = SpaceWarpPlugin.SpaceWarpModInfo.Version,
                        Max = SpaceWarpPlugin.SpaceWarpModInfo.Version,
                    }
                }
            },
        },GetFolder(guid));
        plugin.SWMetadata = descriptor;
        return descriptor;
    }
}