using System.IO;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Newtonsoft.Json;
using UnityEngine;
using SpaceWarp.API.AssetBundles;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
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
            if (Path.GetFileName(Path.GetDirectoryName(folderPath)) == "plugins")
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
        
        InitModUI();
    }

    public static ModListUI ModListUI { get; private set; }
    
    /// <summary>
    /// Initializes the UI for the mod list and configuration menu
    /// </summary>
    private static void InitModUI()
    {
        GameObject modUIObject = new GameObject("Space Warp Mod UI");
        modUIObject.Persist();
    
        modUIObject.transform.SetParent(Chainloader.ManagerObject.transform);
        ModListUI = modUIObject.AddComponent<ModListUI>();
    
        modUIObject.SetActive(true);
    
        GameObject consoleUIObject = new GameObject("Space Warp Console");
        consoleUIObject.Persist();
        consoleUIObject.transform.SetParent(Chainloader.ManagerObject.transform);
        SpaceWarpConsole con = consoleUIObject.AddComponent<SpaceWarpConsole>();
        consoleUIObject.SetActive(true);
    }

    private static GUISkin _skin = null;

    public static GUISkin Skin
    {
        get
        {
            if (_skin)
                AssetManager.TryGetAsset("space_warp/swconsoleui/spacewarpConsole.guiskin", out _skin);
            return _skin;
        }
    }
}