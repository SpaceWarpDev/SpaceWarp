using System;
using System.Collections.Generic;
using System.IO;
using SpaceWarp2.API.Mods;
using SpaceWarp2.API.Mods.JSON;

namespace SpaceWarp2.API.Backend.Modding;

public interface IInternalModRegister
{
    public static IInternalModRegister Instance;
    public IEnumerable<SpaceWarpPluginDescriptor> InternalPluginDescriptors { get; }

    static DirectoryInfo GetFolder(string guid)
    {
        string path = $"./Redux/{guid}";
        var info = new DirectoryInfo(path);
        if (!info.Exists)
        {
            info.Create();
        }

        return info;
    }

    static SpaceWarpPluginDescriptor GetPluginDescriptorForInternalMod(
        Type type,
        string name,
        string guid,
        string version,
        string description,
        string source,
        string? versionCheck = null
    )
    {
        var plugin = new UnloadedMod(type);
        var descriptor = new SpaceWarpPluginDescriptor(plugin, guid, name, new ModInfo
        {
            Spec = SpecVersion.V3_0,
            Description = description,
            ModID = guid,
            Name = name,
            Author = "Redux Team",
            Version = version,
            Source = source,
            Dependencies = new List<DependencyInfo>
            {
                new()
                {
                    ID = SpaceWarpPlugin.SpaceWarpModInfo.ModID,
                    Version = new SupportedVersionsInfo
                    {
                        Min = SpaceWarpPlugin.SpaceWarpModInfo.Version,
                        Max = SpaceWarpPlugin.SpaceWarpModInfo.Version,
                    }
                }
            },
            VersionCheck = versionCheck
        }, GetFolder(guid));
        plugin.SWMetadata = descriptor;
        return descriptor;
    }
}