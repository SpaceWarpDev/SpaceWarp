using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using Mono.Cecil;
using SpaceWarpPatcher;
using Newtonsoft.Json;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Loading;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.API.UI.Appbar;
using SpaceWarp.API.Versions;
using SpaceWarp.Backend.UI.Appbar;
using SpaceWarp.InternalUtilities;
using SpaceWarp.Patching.LoadingActions;
using SpaceWarp.UI.Console;
using SpaceWarp.UI.ModList;
using UnityEngine;

namespace SpaceWarp;

/// <summary>
///     Handles all the SpaceWarp initialization and mod processing.
/// </summary>
internal static class SpaceWarpManager
{
    internal static ManualLogSource Logger;
    internal static ConfigurationManager.ConfigurationManager ConfigurationManager;

    internal static string SpaceWarpFolder;
    // The plugin can be null
    internal static IReadOnlyList<SpaceWarpPluginDescriptor> AllPlugins;
    // internal static IReadOnlyList<BaseUnityPlugin> NonSpaceWarpPlugins;
    // internal static IReadOnlyList<(BaseUnityPlugin, ModInfo)> NonSpaceWarpInfos;

    // internal static IReadOnlyList<(PluginInfo, ModInfo)> DisabledInfoPlugins;
    // internal static IReadOnlyList<PluginInfo> DisabledNonInfoPlugins;
    internal static IReadOnlyList<SpaceWarpPluginDescriptor> DisabledPlugins;
    internal static IReadOnlyList<(string, bool)> PluginGuidEnabledStatus;
    internal static IReadOnlyList<SpaceWarpErrorDescription> ErroredPlugins;
    


    internal static readonly Dictionary<string, bool> ModsOutdated = new();
    internal static readonly Dictionary<string, bool> ModsUnsupported = new();

    private static GUISkin _skin;

    public static ModListController ModListController { get; internal set; }
    
    public static SpaceWarpConsole SpaceWarpConsole { get; internal set; }

    
    [Obsolete("Spacewarps support for IMGUI will not be getting updates, please use UITK instead")]
    internal static GUISkin Skin
    {
        get
        {
            if (!_skin)
            {
                AssetManager.TryGetAsset($"{SpaceWarpPlugin.ModGuid}/swconsoleui/spacewarpconsole.guiskin", out _skin);
            }

            return _skin;
        }
    }

    internal static ConfigFile FindOrCreateConfigFile(string guid)
    {
        var path = $"{Paths.ConfigPath}/{guid}.cfg";
        return new ConfigFile(path, true);
    }
    internal static void GetAllPlugins()
    {
        var pluginGuidEnabledStatus = new List<(string, bool)>();
#pragma warning disable CS0618
        var spaceWarpPlugins = Chainloader.Plugins.OfType<BaseSpaceWarpPlugin>().ToList();
        var modDescriptors = new List<SpaceWarpPluginDescriptor>();
        var ignoredGUIDs = new List<string>();
        var allErroredPlugins = new List<SpaceWarpErrorDescription>();

        GetCodeBasedSpaceWarpPlugins(spaceWarpPlugins, ignoredGUIDs, modDescriptors,allErroredPlugins);

        GetCodeBasedNonSpaceWarpPlugins(spaceWarpPlugins, ignoredGUIDs, modDescriptors,allErroredPlugins);

        var disabledPlugins = new List<SpaceWarpPluginDescriptor>();

        GetDisabledPlugins(disabledPlugins);

        GetCodelessSpaceWarpPlugins(ignoredGUIDs, modDescriptors, allErroredPlugins);
        
        GetBepInExErroredPlugins(ignoredGUIDs,allErroredPlugins,modDescriptors,disabledPlugins);

        ValidateSpec13Dependencies(allErroredPlugins, modDescriptors);

        GetTrueDependencyErrors(allErroredPlugins, modDescriptors, disabledPlugins);

        SetupDisabledPlugins(modDescriptors, allErroredPlugins, disabledPlugins, pluginGuidEnabledStatus);
        
        AllPlugins = modDescriptors;
        DisabledPlugins = disabledPlugins;
        PluginGuidEnabledStatus = pluginGuidEnabledStatus;
        // Now we must do some funky shit :)
        
        ErroredPlugins = allErroredPlugins;
    }

    private static void SetupDisabledPlugins(
        IEnumerable<SpaceWarpPluginDescriptor> modDescriptors,
        IEnumerable<SpaceWarpErrorDescription> allErroredPlugins,
        IEnumerable<SpaceWarpPluginDescriptor> disabledPlugins,
        List<(string, bool)> pluginGuidEnabledStatus)
    {
        pluginGuidEnabledStatus.AddRange(modDescriptors.Select(plugin => (plugin.Guid, true)));

        pluginGuidEnabledStatus.AddRange(allErroredPlugins.Select(plugin => (plugin.Plugin.Guid, true)));

        pluginGuidEnabledStatus.AddRange(disabledPlugins.Select(plugin => (plugin.Guid, false)));
    }

    private static void GetTrueDependencyErrors(ICollection<SpaceWarpErrorDescription> allErroredPlugins,
        ICollection<SpaceWarpPluginDescriptor> modDescriptors, ICollection<SpaceWarpPluginDescriptor> disabledPlugins)
    {
        foreach (var erroredPlugin in allErroredPlugins)
        {
            if (erroredPlugin.MissingDependencies.Count == 0) continue;
            for (var i = erroredPlugin.MissingDependencies.Count - 1; i >= 0; i--)
            {
                var dep = erroredPlugin.MissingDependencies[i];
                if (modDescriptors.Any(x => x.Guid == dep))
                {
                    erroredPlugin.UnsupportedDependencies.Add(dep);
                    erroredPlugin.MissingDependencies.RemoveAt(i);
                } else if (allErroredPlugins.Any(x => x.Plugin.Guid == dep))
                {
                    erroredPlugin.ErroredDependencies.Add(dep);
                    erroredPlugin.MissingDependencies.RemoveAt(i);
                } else if (disabledPlugins.Any(x => x.Guid == dep))
                {
                    erroredPlugin.DisabledDependencies.Add(dep);
                    erroredPlugin.MissingDependencies.RemoveAt(i);
                }
            }
        }
    }

    private static void ValidateSpec13Dependencies(ICollection<SpaceWarpErrorDescription> allErroredPlugins,
        IList<SpaceWarpPluginDescriptor> modDescriptors)
    {
        for (var i = modDescriptors.Count - 1; i >= 0; i--)
        {
            var plugin = modDescriptors[i];
            if (AssertSpec13Dependencies(modDescriptors, plugin, out var missingDependencies)) continue;
            allErroredPlugins.Add(new SpaceWarpErrorDescription(plugin)
            {
                MissingDependencies = missingDependencies
            });
            modDescriptors.RemoveAt(i);
        }
    }

    private static bool AssertSpec13Dependencies(IList<SpaceWarpPluginDescriptor> modDescriptors, SpaceWarpPluginDescriptor plugin,
        out List<string> missingDependencies)
    {
        missingDependencies = new List<string>();
        if (plugin.SWInfo.Spec < SpecVersion.V1_3) return true;
        foreach (var dep in plugin.SWInfo.Dependencies)
        {
            var descriptor = modDescriptors.FirstOrDefault(x => x.Guid == dep.ID);
            if (descriptor == null)
            {
                missingDependencies.Add(dep.ID);
                continue;
            }

            if (!VersionUtility.IsSupported(descriptor.SWInfo.Version, dep.Version.Min, dep.Version.Max))
            {
                missingDependencies.Add(descriptor.Guid);
            }
        }

        return missingDependencies.Count == 0;
    }

    private static void GetBepInExErroredPlugins(IReadOnlyCollection<string> ignoredGuids,
        ICollection<SpaceWarpErrorDescription> errorDescriptions, 
        IReadOnlyCollection<SpaceWarpPluginDescriptor> allLoadedPlugins,
        IReadOnlyCollection<SpaceWarpPluginDescriptor> allDisabledPlugins)
    {
        // Lets do some magic here by copying some code to just get all the plugin types
        var allPlugins = TypeLoader.FindPluginTypes(Paths.PluginPath,
            Chainloader.ToPluginInfo,
            Chainloader.HasBepinPlugins,
            "chainloader");
        foreach (var plugin in allPlugins)
        {
            foreach (var info in plugin.Value)
            {
                info.Location = plugin.Key;
                if (allLoadedPlugins.All(x => x.Guid != info.Metadata.GUID) &&
                    allDisabledPlugins.All(x => x.Guid != info.Metadata.GUID) &&
                    errorDescriptions.All(x => x.Plugin.Guid != info.Metadata.GUID) &&
                    !ignoredGuids.Contains(info.Metadata.GUID))
                {
                    errorDescriptions.Add(new SpaceWarpErrorDescription(new SpaceWarpPluginDescriptor(null,
                        info.Metadata.GUID,
                        info.Metadata.Name,
                        BepInExToSWInfo(info),
                        new DirectoryInfo(Path.GetDirectoryName(info.Location)!)))
                    {
                        MissingDependencies = info.Dependencies.Select(x => x.DependencyGUID)
                            .Where(guid => allLoadedPlugins.All(x => x.Guid != guid))
                            .ToList()
                    });
                }
            }
        }
    }
    
    private static void GetCodelessSpaceWarpPlugins(List<string> ignoredGUIDs,
        List<SpaceWarpPluginDescriptor> spaceWarpInfos,
        ICollection<SpaceWarpErrorDescription> errorDescriptions)
    {
        var codelessInfos = new List<SpaceWarpPluginDescriptor>();
        FindAllCodelessSWInfos(codelessInfos);

        var codelessInfosInOrder =
            new List<SpaceWarpPluginDescriptor>();
        // Check for the dependencies on codelessInfos
        ResolveCodelessPluginDependencyOrder(ignoredGUIDs,
            spaceWarpInfos,
            codelessInfosInOrder,
            codelessInfos,
            errorDescriptions);
        spaceWarpInfos.AddRange(codelessInfosInOrder);
    }
    private static bool CodelessDependencyResolved(SpaceWarpPluginDescriptor descriptor,
        ICollection<SpaceWarpPluginDescriptor> spaceWarpInfos,
        IReadOnlyCollection<SpaceWarpPluginDescriptor> codelessInfosInOrder)
    {
        foreach (var dependency in descriptor.SWInfo.Dependencies)
        {
            Logger.LogInfo($"({descriptor.Name}) Attempting to check if dependency is resolved: {dependency.ID}");
            var info = spaceWarpInfos.FirstOrDefault(x => x.Guid == dependency.ID) ??
                       codelessInfosInOrder.FirstOrDefault(x => x.Guid == dependency.ID);
            if (info == null || !VersionUtility.IsSupported(info.SWInfo.Version, dependency.Version.Min,
                    dependency.Version.Max)) return false;
        }

        return true;
    }
    private static void ResolveCodelessPluginDependencyOrder(ICollection<string> ignoredGUIDs,
        ICollection<SpaceWarpPluginDescriptor> spaceWarpInfos,
        List<SpaceWarpPluginDescriptor> codelessInfosInOrder,
        List<SpaceWarpPluginDescriptor> codelessInfos,
        ICollection<SpaceWarpErrorDescription> errorDescriptions)
    {
        var changed = true;
        while (changed)
        {
            changed = false;
            for (var i = codelessInfos.Count - 1; i >= 0; i--)
            {
                if (!CodelessDependencyResolved(codelessInfos[i],spaceWarpInfos,codelessInfosInOrder)) continue;
                codelessInfosInOrder.Add(codelessInfos[i]);
                codelessInfos.RemoveAt(i);
                changed = true;
            }
        }

        if (codelessInfos.Count <= 0) return;
        foreach (var info in codelessInfos)
        {
            Logger.LogError($"Missing dependency for codeless mod: {info.Name}, this mod will not be loaded");
            errorDescriptions.Add(new SpaceWarpErrorDescription(info)
            {
                MissingDependencies = info.SWInfo.Dependencies.Select(x => x.ID).ToList()
            });

        }
    }

    private static void FindAllCodelessSWInfos(ICollection<SpaceWarpPluginDescriptor> codelessInfos)
    {
        var pluginPath = new DirectoryInfo(Paths.PluginPath);
        foreach (var swinfo in pluginPath.GetFiles("swinfo.json", SearchOption.AllDirectories))
        {
            ModInfo swinfoData;
            try
            {
                swinfoData = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(swinfo.FullName));
            }
            catch
            {
                Logger.LogError($"Error reading metadata file: {swinfo.FullName}, this mod will be ignored");
                continue;
            }

            if (swinfoData.Spec < SpecVersion.V1_3)
            {
                Logger.LogWarning(
                    $"Found swinfo information for: {swinfoData.Name}, but its spec is less than v1.3, if this describes a \"codeless\" mod, it will be ignored");
                continue;
            }

            var guid = swinfoData.ModID;
            // If we already know about this mod, ignore it
            if (Chainloader.PluginInfos.ContainsKey(guid)) continue;

            // Now we can just add it to our plugin list
            codelessInfos.Add(new SpaceWarpPluginDescriptor(null, guid, swinfoData.Name, swinfoData, swinfo.Directory,
                FindOrCreateConfigFile(guid)));
        }
    }

    private static void GetDisabledPlugins(ICollection<SpaceWarpPluginDescriptor> disabledPlugins)
    {
        var allDisabledPlugins = ChainloaderPatch.DisabledPlugins;
        foreach (var plugin in allDisabledPlugins)
        {
            GetSingleDisabledPlugin(disabledPlugins, plugin);
        }
    }

    private static void GetSingleDisabledPlugin(ICollection<SpaceWarpPluginDescriptor> disabledPlugins, PluginInfo plugin)
    {
        var folderPath = Path.GetDirectoryName(plugin.Location);
        var swInfoPath = Path.Combine(folderPath!, "swinfo.json");
        if (Path.GetFileName(folderPath) != "plugins" && File.Exists(swInfoPath))
        {
            try
            {
                var swInfo = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(swInfoPath));
                disabledPlugins.Add(new SpaceWarpPluginDescriptor(null,
                    plugin.Metadata.GUID,
                    plugin.Metadata.Name,
                    swInfo,
                    new DirectoryInfo(folderPath)));
            }
            catch
            {
                disabledPlugins.Add(GetBepInExDescriptor(plugin));
            }
        }
        else
        {
            disabledPlugins.Add(GetBepInExDescriptor(plugin));
        }
    }

    private static void GetCodeBasedNonSpaceWarpPlugins(List<BaseSpaceWarpPlugin> spaceWarpPlugins,
        ICollection<string> ignoredGUIDs,
        ICollection<SpaceWarpPluginDescriptor> allPlugins,
        ICollection<SpaceWarpErrorDescription> errorDescriptions)
    {
        var allBiePlugins = Chainloader.Plugins.ToList();
        foreach (var plugin in allBiePlugins)
        {
            GetSingleBepInExOnlyPlugin(spaceWarpPlugins, ignoredGUIDs, allPlugins, errorDescriptions, plugin);
        }
#pragma warning restore CS0618
    }

    private static void GetSingleBepInExOnlyPlugin(List<BaseSpaceWarpPlugin> spaceWarpPlugins,
        ICollection<string> ignoredGUIDs,
        ICollection<SpaceWarpPluginDescriptor> allPlugins,
        ICollection<SpaceWarpErrorDescription> errorDescriptions,
        BaseUnityPlugin plugin)
    {
        if (spaceWarpPlugins.Contains(plugin as BaseSpaceWarpPlugin))
        {
            return;
        }

        var folderPath = Path.GetDirectoryName(plugin.Info.Location);
        var modInfoPath = Path.Combine(folderPath!, "swinfo.json");
        if (File.Exists(modInfoPath))
        {
            if (!TryReadModInfo(ignoredGUIDs, errorDescriptions, plugin, modInfoPath, folderPath,
                    out var metadata)) return;
            if (!AssertSpecificationCompliance(ignoredGUIDs, errorDescriptions, plugin, metadata, folderPath))
                return;
            allPlugins.Add(new SpaceWarpPluginDescriptor(null, plugin.Info.Metadata.GUID, plugin.Info.Metadata.Name,
                metadata, new DirectoryInfo(Path.GetDirectoryName(plugin.Info.Location)!)));
        }
        else
        {
            allPlugins.Add(GetBepInExDescriptor(plugin));
        }
    }

    private static SpaceWarpPluginDescriptor GetBepInExDescriptor(BaseUnityPlugin plugin)
    {
        return new SpaceWarpPluginDescriptor(null,
            plugin.Info.Metadata.GUID,
            plugin.Info.Metadata.Name,
            BepInExToSWInfo(plugin.Info),
            new DirectoryInfo(Path.GetDirectoryName(plugin.Info.Location)!),plugin.Config);
    }
    
    private static SpaceWarpPluginDescriptor GetBepInExDescriptor(PluginInfo info)
    {
        return new SpaceWarpPluginDescriptor(null,
            info.Metadata.GUID,
            info.Metadata.Name,
            BepInExToSWInfo(info),
            new DirectoryInfo(Path.GetDirectoryName(info.Location)!));
    }

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

    private static void GetCodeBasedSpaceWarpPlugins(List<BaseSpaceWarpPlugin> spaceWarpPlugins,
        ICollection<string> ignoredGUIDs,
        ICollection<SpaceWarpPluginDescriptor> spaceWarpInfos,
        ICollection<SpaceWarpErrorDescription> errorDescriptions)
    {
        foreach (var plugin in spaceWarpPlugins.ToArray())
        {
            GetSingleSpaceWarpPlugin(ignoredGUIDs, spaceWarpInfos, errorDescriptions, plugin);
        }
    }

    private static void GetSingleSpaceWarpPlugin(ICollection<string> ignoredGUIDs, ICollection<SpaceWarpPluginDescriptor> spaceWarpInfos,
        ICollection<SpaceWarpErrorDescription> errorDescriptions, BaseSpaceWarpPlugin plugin)
    {
        var folderPath = Path.GetDirectoryName(plugin.Info.Location);
        plugin.PluginFolderPath = folderPath;
        if (!AssertFolderPath(ignoredGUIDs, errorDescriptions, plugin, folderPath)) return;

        var modInfoPath = Path.Combine(folderPath!, "swinfo.json");

        if (!AssertModInfoExistence(ignoredGUIDs, errorDescriptions, plugin, modInfoPath, folderPath)) return;

        if (!TryReadModInfo(ignoredGUIDs, errorDescriptions, plugin, modInfoPath, folderPath, out var metadata)) return;

        if (!AssertSpecificationCompliance(ignoredGUIDs, errorDescriptions, plugin, metadata, folderPath)) return;

        plugin.SpaceWarpMetadata = metadata;
        var directoryInfo = new FileInfo(modInfoPath).Directory;
        spaceWarpInfos.Add(new SpaceWarpPluginDescriptor(plugin,
            plugin.Info.Metadata.GUID,
            metadata.Name,
            metadata,
            directoryInfo,plugin.Config));
    }

    private static bool AssertSpecificationCompliance(ICollection<string> ignoredGUIDs,
        ICollection<SpaceWarpErrorDescription> errorDescriptions,
        BaseUnityPlugin plugin,
        ModInfo metadata,
        string folderPath) =>
        metadata.Spec < SpecVersion.V1_3 ||
        AssertSpecVersion13Compliance(ignoredGUIDs,
            errorDescriptions,
            plugin,
            metadata,
            folderPath);

    private static bool AssertSpecVersion13Compliance(ICollection<string> ignoredGUIDs,
        ICollection<SpaceWarpErrorDescription> errorDescriptions,
        BaseUnityPlugin plugin,
        ModInfo metadata,
        string folderPath) =>
        AssertMatchingModID(ignoredGUIDs, errorDescriptions, plugin, metadata, folderPath) &&
        AssertMatchingVersions(ignoredGUIDs, errorDescriptions, plugin, metadata, folderPath) &&
        AssertAllDependenciesAreSpecified(errorDescriptions, plugin, metadata, folderPath);

    private static bool AssertAllDependenciesAreSpecified(ICollection<SpaceWarpErrorDescription> errorDescriptions,
        BaseUnityPlugin plugin,
        ModInfo metadata,
        string folderPath)
    {
        var unspecifiedDeps = new List<string>();
        foreach (var dep in plugin.Info.Dependencies)
        {
            AssertDependencyIsSpecified(plugin, dep, metadata, unspecifiedDeps);
        }

        if (unspecifiedDeps.Count <= 0) return true;
        errorDescriptions.Add(new SpaceWarpErrorDescription(new SpaceWarpPluginDescriptor(plugin as BaseSpaceWarpPlugin, 
            plugin.Info.Metadata.GUID,
            plugin.Info.Metadata.Name,
            metadata,
            new DirectoryInfo(folderPath)))
        {
            UnspecifiedDependencies = unspecifiedDeps
        });
        return false;

    }

    private static void AssertDependencyIsSpecified(BaseUnityPlugin plugin,
        BepInDependency dep,
        ModInfo metadata,
        ICollection<string> unspecifiedDeps)
    {
        if (metadata.Dependencies.Any(x => x.ID == dep.DependencyGUID)) return;
        Logger.LogError(
            $"Found Space Warp Plugin {plugin.Info.Metadata.Name} that has an unspecified swinfo dependency found in its BepInDependencies: {dep.DependencyGUID}");
        unspecifiedDeps.Add(dep.DependencyGUID);
        metadata.Dependencies.Add(new DependencyInfo
        {
            ID = dep.DependencyGUID,
            Version = new SupportedVersionsInfo
            {
                Min = dep.MinimumVersion.ToString(),
                Max = "*"
            }
        });
    }

    private static bool AssertMatchingVersions(ICollection<string> ignoredGUIDs, ICollection<SpaceWarpErrorDescription> errorDescriptions,
        BaseUnityPlugin plugin, ModInfo metadata, string folderPath)
    {
        if (new Version(metadata.Version) == plugin.Info.Metadata.Version) return true;
        Logger.LogError(
            $"Found Space Warp plugin {plugin.Info.Metadata.Name} that's swinfo version does not match the plugin version, this mod will not be initialized");
        ignoredGUIDs.Add(plugin.Info.Metadata.GUID);
        errorDescriptions.Add(new SpaceWarpErrorDescription(new SpaceWarpPluginDescriptor(plugin as BaseSpaceWarpPlugin, 
            plugin.Info.Metadata.GUID,
            plugin.Info.Metadata.Name,
            metadata,
            new DirectoryInfo(folderPath)))
        {
            MismatchedVersion = true
        });
        return false;

    }

    private static bool AssertMatchingModID(ICollection<string> ignoredGUIDs, ICollection<SpaceWarpErrorDescription> errorDescriptions,
        BaseUnityPlugin plugin, ModInfo metadata, string folderPath)
    {
        var modID = metadata.ModID;
        if (modID == plugin.Info.Metadata.GUID) return true;
        Logger.LogError(
            $"Found Space Warp plugin {plugin.Info.Metadata.Name} that has an swinfo.json w/ spec version >= 1.3 that's ModID is not the same as the plugins GUID, This mod will not be initialized.");
        ignoredGUIDs.Add(plugin.Info.Metadata.GUID);
        errorDescriptions.Add(new SpaceWarpErrorDescription(new SpaceWarpPluginDescriptor(plugin as BaseSpaceWarpPlugin, 
            plugin.Info.Metadata.GUID,
            plugin.Info.Metadata.Name,
            metadata,
            new DirectoryInfo(folderPath)))
        {
            BadID = true
        });
        return false;

    }

    private static bool TryReadModInfo(ICollection<string> ignoredGUIDs, ICollection<SpaceWarpErrorDescription> errorDescriptions, BaseUnityPlugin plugin,
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
            ignoredGUIDs.Add(plugin.Info.Metadata.GUID);
            errorDescriptions.Add(new SpaceWarpErrorDescription(new SpaceWarpPluginDescriptor(
                plugin as BaseSpaceWarpPlugin, 
                plugin.Info.Metadata.GUID,
                plugin.Info.Metadata.Name,
                BepInExToSWInfo(plugin.Info),
                new DirectoryInfo(folderPath)))
            {
                MissingSwinfo = true
            });
            metadata = null;
            return false;
        }

        return true;
    }

    private static bool AssertModInfoExistence(ICollection<string> ignoredGUIDs, ICollection<SpaceWarpErrorDescription> errorDescriptions,
        BaseSpaceWarpPlugin plugin, string modInfoPath, string folderPath)
    {
        if (File.Exists(modInfoPath)) return true;
        Logger.LogError(
            $"Found Space Warp plugin {plugin.Info.Metadata.Name} without a swinfo.json next to it. This mod will not be initialized.");
        errorDescriptions.Add(new SpaceWarpErrorDescription(new SpaceWarpPluginDescriptor(plugin,
            plugin.Info.Metadata.GUID,
            plugin.Info.Metadata.Name,
            BepInExToSWInfo(plugin.Info),
            new DirectoryInfo(folderPath)))
        {
            MissingSwinfo = true
        });
        ignoredGUIDs.Add(plugin.Info.Metadata.GUID);
        return false;

    }

    private static bool AssertFolderPath(ICollection<string> ignoredGUIDs, ICollection<SpaceWarpErrorDescription> errorDescriptions,
        BaseSpaceWarpPlugin plugin, string folderPath)
    {
        if (Path.GetFileName(folderPath) != "plugins") return true;
        Logger.LogError(
            $"Found Space Warp mod {plugin.Info.Metadata.Name} in the BepInEx/plugins directory. This mod will not be initialized.");
        errorDescriptions.Add(new SpaceWarpErrorDescription(new SpaceWarpPluginDescriptor(plugin,
            plugin.Info.Metadata.GUID,
            plugin.Info.Metadata.Name,
            BepInExToSWInfo(plugin.Info),
            new DirectoryInfo(folderPath)))
        {
            BadDirectory = true
        });
        ignoredGUIDs.Add(plugin.Info.Metadata.GUID);
        return false;

    }

    public static void Initialize(SpaceWarpPlugin spaceWarpPlugin)
    {
        Logger = SpaceWarpPlugin.Logger;

        SpaceWarpFolder = Path.GetDirectoryName(spaceWarpPlugin.Info.Location);

        AppbarBackend.AppBarInFlightSubscriber.AddListener(Appbar.LoadAllButtons);
        AppbarBackend.AppBarOABSubscriber.AddListener(Appbar.LoadOABButtons);
    }


    internal static void CheckKspVersions()
    {
        var kspVersion = typeof(VersionID).GetField("VERSION_TEXT", BindingFlags.Static | BindingFlags.Public)
            ?.GetValue(null) as string;
        foreach (var plugin in AllPlugins)
        {
            // CheckModKspVersion(plugin.Info.Metadata.GUID, plugin.SpaceWarpMetadata, kspVersion);
            CheckModKspVersion(plugin.Guid, plugin.SWInfo, kspVersion);
        }

        foreach (var info in DisabledPlugins)
        {
            CheckModKspVersion(info.Guid, info.SWInfo, kspVersion);
        }
    }

    private static void CheckModKspVersion(string guid, ModInfo modInfo, string kspVersion)
    {
        var unsupported = true;
        try
        {
            unsupported = !modInfo.SupportedKsp2Versions.IsSupported(kspVersion);
        }
        catch (Exception e)
        {
            Logger.LogError($"Unable to check KSP version for {guid} due to error {e}");
        }

        ModsUnsupported[guid] = unsupported;
    }



    internal static void InitializeSpaceWarpsLoadingActions()
    {
        Loading.AddAssetLoadingAction("bundles", "loading asset bundles", FunctionalLoadingActions.AssetBundleLoadingAction, "bundle");
        Loading.AddAssetLoadingAction("images", "loading images", FunctionalLoadingActions.ImageLoadingAction);
    }
}