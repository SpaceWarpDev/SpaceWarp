using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using SpaceWarpPatcher;
using Newtonsoft.Json;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Loading;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.API.UI.Appbar;
using SpaceWarp.API.Versions;
using SpaceWarp.Backend.UI.Appbar;
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

    internal static void GetAllPlugins()
    {
        var pluginGuidEnabledStatus = new List<(string, bool)>();
#pragma warning disable CS0618
        var spaceWarpPlugins = Chainloader.Plugins.OfType<BaseSpaceWarpPlugin>().ToList();
        var modDescriptors = new List<SpaceWarpPluginDescriptor>();
        var ignoredGUIDs = new List<string>();

        GetCodeBasedSpaceWarpPlugins(spaceWarpPlugins, ignoredGUIDs, modDescriptors, pluginGuidEnabledStatus);

        GetCodeBasedNonSpaceWarpPlugins(spaceWarpPlugins, modDescriptors, pluginGuidEnabledStatus);

        var disabledPlugins = new List<SpaceWarpPluginDescriptor>();

        GetDisabledPlugins(disabledPlugins, pluginGuidEnabledStatus);

        GetCodelessSpaceWarpPlugins(ignoredGUIDs, modDescriptors, pluginGuidEnabledStatus);
        AllPlugins = modDescriptors;
        DisabledPlugins = disabledPlugins;
        PluginGuidEnabledStatus = pluginGuidEnabledStatus;
    }

    private static void GetCodelessSpaceWarpPlugins(List<string> ignoredGUIDs, List<SpaceWarpPluginDescriptor> spaceWarpInfos,List<(string, bool)> pluginGuidEnabledStatus)
    {
        var codelessInfos = new List<SpaceWarpPluginDescriptor>();
        FindAllCodelessSWInfos(codelessInfos);

        var codelessInfosInOrder =
            new List<SpaceWarpPluginDescriptor>();
        // Check for the dependencies on codelessInfos
        ResolveCodelessPluginDependencyOrder(ignoredGUIDs, codelessInfosInOrder, codelessInfos,pluginGuidEnabledStatus);
        spaceWarpInfos.AddRange(codelessInfosInOrder);
    }

    private static void ResolveCodelessPluginDependencyOrder(List<string> ignoredGUIDs, List<SpaceWarpPluginDescriptor> codelessInfosInOrder,
        List<SpaceWarpPluginDescriptor> codelessInfos, List<(string, bool)> pluginGuidEnabledStatus)
    {
        bool CodelessDependencyResolved(SpaceWarpPluginDescriptor descriptor)
        {
            foreach (var dependency in descriptor.SWInfo.Dependencies)
            {
                if (Chainloader.PluginInfos.ContainsKey(dependency.ID) && !ignoredGUIDs.Contains(dependency.ID))
                {
                    var chainloaderVersion = Chainloader.PluginInfos[dependency.ID].Metadata.Version.ToString();
                    if (!VersionUtility.IsSupported(chainloaderVersion, dependency.Version.Min, dependency.Version.Max))
                        return false;
                }

                var codelessInfo = codelessInfosInOrder.FirstOrDefault(x => x.Guid == dependency.ID);
                if (codelessInfo == null || !VersionUtility.IsSupported(codelessInfo.SWInfo.Version, dependency.Version.Min,
                        dependency.Version.Max)) return false;
            }

            return true;
        }

        bool changed = true;
        while (changed)
        {
            changed = false;
            for (var i = codelessInfos.Count - 1; i >= 0; i--)
            {
                if (!CodelessDependencyResolved(codelessInfos[i])) continue;
                codelessInfosInOrder.Add(codelessInfos[i]);
                pluginGuidEnabledStatus.Add((codelessInfos[i].Guid,true));
                codelessInfos.RemoveAt(i);
                changed = true;
            }
        }

        if (codelessInfos.Count > 0)
        {
            foreach (var info in codelessInfos)
            {
                // TODO: Specific dependency warnings in the mod list at some point!
                Logger.LogError($"Missing dependency for codeless mod: {info.SWInfo.Name}, this mod will not be loaded");
                
            }
        }
    }

    private static void FindAllCodelessSWInfos(List<SpaceWarpPluginDescriptor> codelessInfos)
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
            codelessInfos.Add(new(null, guid,swinfoData.Name,swinfoData, swinfo.Directory));
        }
    }

    private static void GetDisabledPlugins(List<SpaceWarpPluginDescriptor> disabledPlugins,
        List<(string, bool)> pluginGuidEnabledStatus)
    {
        var allDisabledPlugins = ChainloaderPatch.DisabledPlugins;
        foreach (var plugin in allDisabledPlugins)
        {
            var folderPath = Path.GetDirectoryName(plugin.Location);
            var swInfoPath = Path.Combine(folderPath!, "swinfo.json");
            if (Path.GetFileName(folderPath) != "plugins" && File.Exists(swInfoPath))
            {
                try
                {
                    var swInfo = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(swInfoPath));
                    disabledPlugins.Add(new SpaceWarpPluginDescriptor(null,plugin.Metadata.GUID,plugin.Metadata.Name,swInfo,new DirectoryInfo(folderPath)));
                }
                catch
                {
                    var swInfo = BepinexToSWInfo(plugin);
                    disabledPlugins.Add(new SpaceWarpPluginDescriptor(null,plugin.Metadata.GUID,plugin.Metadata.Name,swInfo,new DirectoryInfo(folderPath)));
                }
            }
            else
            {
                var swInfo = BepinexToSWInfo(plugin);
                disabledPlugins.Add(new SpaceWarpPluginDescriptor(null,plugin.Metadata.GUID,plugin.Metadata.Name,swInfo,new DirectoryInfo(folderPath)));
            }

            pluginGuidEnabledStatus.Add((plugin.Metadata.GUID, false));
        }
    }

    private static void GetCodeBasedNonSpaceWarpPlugins(List<BaseSpaceWarpPlugin> spaceWarpPlugins, List<SpaceWarpPluginDescriptor> allPlugins, List<(string, bool)> pluginGuidEnabledStatus)
    {
        var allBEPlugins = Chainloader.Plugins.ToList();
        // List<BaseUnityPlugin> nonSWPlugins = new();
        // List<(BaseUnityPlugin, ModInfo)> nonSWInfos = new();
        foreach (var plugin in allBEPlugins)
        {
            if (spaceWarpPlugins.Contains(plugin as BaseSpaceWarpPlugin))
            {
                continue;
            }

            var folderPath = Path.GetDirectoryName(plugin.Info.Location);
            var modInfoPath = Path.Combine(folderPath!, "swinfo.json");
            if (File.Exists(modInfoPath))
            {
                var info = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(modInfoPath));
                // nonSWInfos.Add((plugin, info));
                allPlugins.Add(new SpaceWarpPluginDescriptor(null,plugin.Info.Metadata.GUID,plugin.Info.Metadata.Name, info, new DirectoryInfo(Path.GetDirectoryName(plugin.Info.Location)!)));
            }
            else
            {
                var newInfo = BepinexToSWInfo(plugin.Info);
                allPlugins.Add(new SpaceWarpPluginDescriptor(null,plugin.Info.Metadata.GUID,plugin.Info.Metadata.Name,newInfo,new DirectoryInfo(Path.GetDirectoryName(plugin.Info.Location)!)));
            }

            pluginGuidEnabledStatus.Add((plugin.Info.Metadata.GUID, true));
        }

        // NonSpaceWarpPlugins = nonSWPlugins;
        // NonSpaceWarpInfos = nonSWInfos;
#pragma warning restore CS0618
    }

    private static ModInfo BepinexToSWInfo(PluginInfo plugin)
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

    private static void GetCodeBasedSpaceWarpPlugins(List<BaseSpaceWarpPlugin> spaceWarpPlugins, List<string> ignoredGUIDs, List<SpaceWarpPluginDescriptor> spaceWarpInfos,
        List<(string, bool)> pluginGuidEnabledStatus)
    {
        foreach (var plugin in spaceWarpPlugins.ToArray())
        {
            var folderPath = Path.GetDirectoryName(plugin.Info.Location);
            plugin.PluginFolderPath = folderPath;
            if (Path.GetFileName(folderPath) == "plugins")
            {
                Logger.LogError(
                    $"Found Space Warp mod {plugin.Info.Metadata.Name} in the BepInEx/plugins directory. This mod will not be initialized.");
                ignoredGUIDs.Add(plugin.Info.Metadata.GUID);
                continue;
            }

            var modInfoPath = Path.Combine(folderPath!, "swinfo.json");

            if (!File.Exists(modInfoPath))
            {
                Logger.LogError(
                    $"Found Space Warp plugin {plugin.Info.Metadata.Name} without a swinfo.json next to it. This mod will not be initialized.");
                ignoredGUIDs.Add(plugin.Info.Metadata.GUID);
                continue;
            }

            ModInfo metadata;
            try
            {
                metadata = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(modInfoPath));
            }
            catch
            {
                Logger.LogError(
                    $"Error reading metadata for spacewarp plugin {plugin.Info.Metadata.Name}. This mod will not be initialized");
                ignoredGUIDs.Add(plugin.Info.Metadata.GUID);
                continue;
            }

            if (metadata.Spec >= SpecVersion.V1_3)
            {
                // Enforce Mod ID here
                var modID = metadata.ModID;
                if (modID != plugin.Info.Metadata.GUID)
                {
                    Logger.LogError(
                        $"Found Space Warp plugin {plugin.Info.Metadata.Name} that has an swinfo.json w/ spec version >= 1.3 that's ModID is not the same as the plugins GUID, This mod will not be initialized.");
                    ignoredGUIDs.Add(plugin.Info.Metadata.GUID);
                    continue;
                }

                // We should also enforce equality between versions w/ this spec
                if (new Version(metadata.Version) != plugin.Info.Metadata.Version)
                {
                    Logger.LogError(
                        $"Found Space Warp plugin {plugin.Info.Metadata.Name} that's swinfo version does not match the plugin version, this mod will not be initialized");
                    ignoredGUIDs.Add(plugin.Info.Metadata.GUID);
                    continue;
                }
            }


            plugin.SpaceWarpMetadata = metadata;
            var directoryInfo = new FileInfo(modInfoPath).Directory;
            spaceWarpInfos.Add(new SpaceWarpPluginDescriptor(plugin,
                plugin.Info.Metadata.GUID,
                metadata.Name,
                metadata,
                directoryInfo));
            pluginGuidEnabledStatus.Add((plugin.Info.Metadata.GUID, true));
        }
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