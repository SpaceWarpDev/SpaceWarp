using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using Newtonsoft.Json;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Logging;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;

namespace SpaceWarp.Backend.Mods;

internal static class PluginRegister
{
    

    public static void RegisterAllMods()
    {
        RegisterAllBepInExMods();
        RegisterAllCodelessMods();
        RegisterAllKspMods();
        RegisterAllErroredMods();
    }
    
    private static ILogger Logger = (BepInExLogger)SpaceWarpPlugin.Logger;
    private static ModInfo BepInExToSWInfo(PluginInfo plugin)
    {
        var newInfo = new ModInfo
        {
            Spec = SpecVersion.V1_3,
            ModID = plugin.Metadata.GUID,
            Name = plugin.Metadata.Name,
            Author = "<unknown>",
            Description = "<unknown>",
            Source = "<unknown>",
            Version = plugin.Metadata.Version.ToString(),
            Dependencies = plugin.Dependencies.Select(x => new DependencyInfo
            {
                ID = x.DependencyGUID,
                Version = new SupportedVersionsInfo
                {
                    Min = x.MinimumVersion.ToString(),
                    Max = "*"
                }
            }).ToList(),
            SupportedKsp2Versions = new SupportedVersionsInfo
            {
                Min = "*",
                Max = "*"
            },
            VersionCheck = null,
            VersionCheckType = VersionCheckType.SwInfo
        };
        return newInfo;
    }
    
    private static bool AssertFolderPath(BaseSpaceWarpPlugin plugin, string folderPath)
    {
        if (Path.GetFileName(folderPath) != "plugins") return true;
        Logger.LogError(
            $"Found Space Warp mod {plugin.Info.Metadata.Name} in the BepInEx/plugins directory. This mod will not be initialized.");
        
        var descriptor = new SpaceWarpPluginDescriptor(plugin,
            plugin.Info.Metadata.GUID,
            plugin.Info.Metadata.Name,
            BepInExToSWInfo(plugin.Info),
            new DirectoryInfo(folderPath),false, new BepInExConfigFile(plugin.Config));
        PluginList.NoteBadDirectoryError(descriptor);
        return false;

    }
    
    private static bool AssertModInfoExistence(BaseSpaceWarpPlugin plugin, string modInfoPath, string folderPath)
    {
        if (File.Exists(modInfoPath)) return true;
        Logger.LogError(
            $"Found Space Warp plugin {plugin.Info.Metadata.Name} without a swinfo.json next to it. This mod will not be initialized.");
        PluginList.NoteMissingSwinfoError(new SpaceWarpPluginDescriptor(plugin,
            plugin.Info.Metadata.GUID,
            plugin.Info.Metadata.Name,
            BepInExToSWInfo(plugin.Info),
            new DirectoryInfo(folderPath)));
        return false;
    }
    
    private static bool TryReadModInfo(BaseUnityPlugin plugin,
        string modInfoPath, string folderPath, out ModInfo metadata)
    {
        try
        {
            metadata = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(modInfoPath));
        }
        catch
        {
            Logger.LogError(
                $"Error reading metadata for spacewarp plugin {plugin.Info.Metadata.Name}. This mod will not be initialized");
            PluginList.NoteMissingSwinfoError(new SpaceWarpPluginDescriptor(
                plugin as BaseSpaceWarpPlugin,
                plugin.Info.Metadata.GUID,
                plugin.Info.Metadata.Name,
                BepInExToSWInfo(plugin.Info),
                new DirectoryInfo(folderPath)));
            metadata = null;
            return false;
        }

        return true;
    }
    
    private static bool AssertSpecificationCompliance(
        BaseUnityPlugin plugin,
        ModInfo metadata,
        string folderPath) =>
        metadata.Spec < SpecVersion.V1_3 ||
        AssertSpecVersion13Compliance(
            plugin,
            metadata,
            folderPath);

    private static bool AssertSpecVersion13Compliance(
        BaseUnityPlugin plugin,
        ModInfo metadata,
        string folderPath) =>
        AssertMatchingModID(plugin, metadata, folderPath) &&
        AssertMatchingVersions(plugin, metadata, folderPath) &&
        AssertAllDependenciesAreSpecified(plugin, metadata, folderPath);
    
    
    private static bool AssertMatchingVersions(BaseUnityPlugin plugin, ModInfo metadata, string folderPath)
    {
        if (new Version(metadata.Version) == plugin.Info.Metadata.Version) return true;
        Logger.LogError(
            $"Found Space Warp plugin {plugin.Info.Metadata.Name} that's swinfo version does not match the plugin version, this mod will not be initialized");
        PluginList.NoteMismatchedVersionError(new SpaceWarpPluginDescriptor(plugin as BaseSpaceWarpPlugin,
            plugin.Info.Metadata.GUID,
            plugin.Info.Metadata.Name,
            metadata,
            new DirectoryInfo(folderPath)));
        return false;

    }
    
    private static bool AssertMatchingModID(BaseUnityPlugin plugin, ModInfo metadata, string folderPath)
    {
        var modID = metadata.ModID;
        if (modID == plugin.Info.Metadata.GUID) return true;
        Logger.LogError(
            $"Found Space Warp plugin {plugin.Info.Metadata.Name} that has an swinfo.json w/ spec version >= 1.3 that's ModID is not the same as the plugins GUID, This mod will not be initialized.");
        PluginList.NoteBadIDError(new SpaceWarpPluginDescriptor(
            plugin as BaseSpaceWarpPlugin,
            plugin.Info.Metadata.GUID,
            plugin.Info.Metadata.Name,
            metadata,
            new DirectoryInfo(folderPath)));
        return false;

    }
    
    private static void RegisterSingleSpaceWarpPlugin(BaseSpaceWarpPlugin plugin)
    {
        var folderPath = Path.GetDirectoryName(plugin.Info.Location);
        plugin.PluginFolderPath = folderPath;
        if (!AssertFolderPath(plugin, folderPath)) return;

        var modInfoPath = Path.Combine(folderPath!, "swinfo.json");

        if (!AssertModInfoExistence(plugin, modInfoPath, folderPath)) return;

        if (!TryReadModInfo(plugin, modInfoPath, folderPath, out var metadata)) return;

        if (!AssertSpecificationCompliance(plugin, metadata, folderPath)) return;

        plugin.SpaceWarpMetadata = metadata;
        var directoryInfo = new FileInfo(modInfoPath).Directory;
        var descriptor = new SpaceWarpPluginDescriptor(plugin,
            plugin.Info.Metadata.GUID,
            metadata.Name,
            metadata,
            directoryInfo,true, new BepInExConfigFile(plugin.Config));
        PluginList.RegisterPlugin(descriptor);
    }
    private static void RegisterAllBepInExMods()
    {
        foreach (var plugin in Chainloader.PluginInfos)
        {
            var guid = plugin.Key;
            var info = plugin.Value;
            var instance = info.Instance;
            
            if (instance is SpaceWarpPlugin spaceWarpMod)
            {
                RegisterSingleSpaceWarpPlugin(spaceWarpMod);
            }
            else
            {
                
            }
        }
    }

    private static void RegisterAllCodelessMods()
    {
        
    }

    private static void RegisterAllKspMods()
    {
        
    }

    private static void RegisterAllErroredMods()
    {
        
    }
}