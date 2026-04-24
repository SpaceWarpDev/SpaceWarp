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

    public IEnumerable<SpaceWarpPluginDescriptor> Ksp1PluginDescriptors =>
        Array.Empty<SpaceWarpPluginDescriptor>();

    static DirectoryInfo GetFolder(string guid)
    {
        string path = $"./Redux/Config/{guid}";
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

    /// <summary>
    /// Builds a <see cref="SpaceWarpPluginDescriptor"/> for a KSP1 mod surfaced through the
    /// KSP1 mod importer. These descriptors don't host a real plugin; they exist so the mod
    /// shows up under the "KSP1 Mods" foldout in the SpaceWarp mod list.
    /// </summary>
    static SpaceWarpPluginDescriptor GetPluginDescriptorForKsp1Mod(
        string name,
        DirectoryInfo folder,
        string? description = null,
        string version = "?",
        string author = ""
    )
    {
        string guid = $"ksp1.{name}";
        var descriptor = new SpaceWarpPluginDescriptor(
            plugin: null,
            guid: guid,
            name: name,
            swInfo: new ModInfo
            {
                Spec = SpecVersion.V3_0,
                Description = description ?? string.Empty,
                ModID = guid,
                Name = name,
                Author = author,
                Version = version,
                Source = string.Empty,
                Dependencies = new List<DependencyInfo>(),
                VersionCheck = null,
            },
            folder: folder,
            doLoadingActions: false,
            configFile: null
        )
        {
            IsKsp1 = true,
        };
        return descriptor;
    }
}