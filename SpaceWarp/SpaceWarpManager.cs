using System.IO;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Newtonsoft.Json;
using UnityEngine;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.API.UI.Appbar;
using SpaceWarp.Backend.UI.Appbar;
using SpaceWarp.UI;

namespace SpaceWarp;

/// <summary>
/// Handles all the SpaceWarp initialization and mod processing.
/// </summary>
internal static class SpaceWarpManager
{
    internal static ManualLogSource Logger;

    internal static string SpaceWarpFolder;
    
    internal static IReadOnlyList<BaseSpaceWarpPlugin> SpaceWarpPlugins;

    internal static void GetSpaceWarpPlugins()
    {
        // obsolete warning for Chainloader.Plugins, is fine since we need ordered list
        // to break this we would likely need to upgrade to BIE 6, which isn't happening
#pragma warning disable CS0618
        var spaceWarpPlugins = Chainloader.Plugins.OfType<BaseSpaceWarpPlugin>().ToList();
        SpaceWarpPlugins = spaceWarpPlugins;
#pragma warning restore CS0618
        foreach (var plugin in SpaceWarpPlugins.ToArray())
        {
            var folderPath = Path.GetDirectoryName(plugin.Info.Location);
            plugin.PluginFolderPath = folderPath;
            if (Path.GetFileName(folderPath) == "plugins")
            {
                Logger.LogError($"Found Space Warp mod {plugin.Info.Metadata.Name} in the BepInEx/plugins directory. This mod will not be initialized.");
                spaceWarpPlugins.Remove(plugin);
                continue;
            }
            var modInfoPath = Path.Combine(folderPath!, "swinfo.json");
            if (!File.Exists(modInfoPath))
            {
                Logger.LogError($"Found Space Warp plugin {plugin.Info.Metadata.Name} without a swinfo.json next to it. This mod will not be initialized.");
                spaceWarpPlugins.Remove(plugin);
                continue;
            }
            plugin.SpaceWarpMetadata = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(modInfoPath));
        }
    }

    public static void Initialize(SpaceWarpPlugin sw)
    {
        Logger = sw.Logger;

        SpaceWarpFolder = Path.GetDirectoryName(sw.Info.Location);

        AppbarBackend.AppBarInFlightSubscriber.AddListener(Appbar.LoadAllButtons);
    }

    public static ModListUI ModListUI { get; internal set; }

    private static GUISkin _skin;

    public static GUISkin Skin
    {
        get
        {
            if (!_skin)
                AssetManager.TryGetAsset("spacewarp/swconsoleui/spacewarpconsole.guiskin", out _skin);
            return _skin;
        }
    }
}