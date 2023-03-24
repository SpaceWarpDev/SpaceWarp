using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Newtonsoft.Json;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.API.UI.Appbar;
using SpaceWarp.Backend.UI.Appbar;
using SpaceWarp.UI;
using UnityEngine;

namespace SpaceWarp;

/// <summary>
///     Handles all the SpaceWarp initialization and mod processing.
/// </summary>
internal static class SpaceWarpManager
{
    internal static ManualLogSource Logger;
    internal static string SpaceWarpFolder;
    internal static IReadOnlyList<BaseSpaceWarpPlugin> SpaceWarpPlugins;
    internal static IReadOnlyList<BaseUnityPlugin> NonSpaceWarpPlugins;
    internal static IReadOnlyList<ModInfo> NonSpaceWarpInfos;
    internal static ConfigurationManager.ConfigurationManager ConfigurationManager;
    internal static readonly Dictionary<string, bool> ModsOutdated = new();
    internal static readonly Dictionary<string, bool> ModsUnsupported = new();

    private static GUISkin _skin;

    public static ModListUI ModListUI { get; internal set; }

    public static GUISkin Skin
    {
        get
        {
            if (!_skin)
            {
                AssetManager.TryGetAsset("spacewarp/swconsoleui/spacewarpconsole.guiskin", out _skin);
            }

            return _skin;
        }
    }

    internal static void GetSpaceWarpPlugins()
    {
        // obsolete warning for Chainloader.Plugins, is fine since we need ordered list
        // to break this we would likely need to upgrade to BIE 6, which isn't happening
#pragma warning disable CS0618
        var spaceWarpPlugins = Chainloader.Plugins.OfType<BaseSpaceWarpPlugin>().ToList();
        SpaceWarpPlugins = spaceWarpPlugins;

        foreach (var plugin in SpaceWarpPlugins.ToArray())
        {
            var folderPath = Path.GetDirectoryName(plugin.Info.Location);
            plugin.PluginFolderPath = folderPath;
            if (Path.GetFileName(folderPath) == "plugins")
            {
                Logger.LogError(
                    $"Found Space Warp mod {plugin.Info.Metadata.Name} in the BepInEx/plugins directory. This mod will not be initialized.");
                spaceWarpPlugins.Remove(plugin);
                continue;
            }

            var modInfoPath = Path.Combine(folderPath!, "swinfo.json");
            if (!File.Exists(modInfoPath))
            {
                Logger.LogError(
                    $"Found Space Warp plugin {plugin.Info.Metadata.Name} without a swinfo.json next to it. This mod will not be initialized.");
                spaceWarpPlugins.Remove(plugin);
                continue;
            }

            plugin.SpaceWarpMetadata = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(modInfoPath));
        }

        var allPlugins = Chainloader.Plugins.ToList();
        List<BaseUnityPlugin> nonSWPlugins = new();
        List<ModInfo> nonSWInfos = new();
        foreach (var plugin in allPlugins)
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
                nonSWInfos.Add(info);
            }
            else
            {
                nonSWPlugins.Add(plugin);
            }
        }
#pragma warning restore CS0618
        NonSpaceWarpPlugins = nonSWPlugins;
        NonSpaceWarpInfos = nonSWInfos;
    }

    public static void Initialize(SpaceWarpPlugin spaceWarpPlugin)
    {
        Logger = spaceWarpPlugin.Logger;

        SpaceWarpFolder = Path.GetDirectoryName(spaceWarpPlugin.Info.Location);

        AppbarBackend.AppBarInFlightSubscriber.AddListener(Appbar.LoadAllButtons);
        AppbarBackend.AppBarOABSubscriber.AddListener(Appbar.LoadOABButtons);
    }


    internal static void CheckKspVersions()
    {
        var kspVersion = typeof(VersionID).GetField("VERSION_TEXT", BindingFlags.Static | BindingFlags.Public)
            ?.GetValue(null) as string;
        foreach (var plugin in SpaceWarpPlugins)
        {
            ModsUnsupported[plugin.SpaceWarpMetadata.ModID] =
                !plugin.SpaceWarpMetadata.SupportedKsp2Versions.IsSupported(kspVersion);
        }

        foreach (var info in NonSpaceWarpInfos)
        {
            ModsUnsupported[info.ModID] = !info.SupportedKsp2Versions.IsSupported(kspVersion);
        }
    }
}