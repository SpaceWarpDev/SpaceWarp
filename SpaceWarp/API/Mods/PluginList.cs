using System.Collections.Generic;
using BepInEx;
using BepInEx.Bootstrap;

// Disable obsolete warning for Chainloader.Plugins
#pragma warning disable CS0618

namespace SpaceWarp.API.Mods;

/// <summary>
/// API for accessing information about currently loaded and disabled plugins.
/// </summary>
public static class PluginList
{
    /// <summary>
    /// Contains information about all currently loaded plugins. The key is the BepInEx GUID of the plugin.
    /// </summary>
    public static Dictionary<string, PluginInfo> LoadedPluginInfos { get; } = Chainloader.PluginInfos;

    /// <summary>
    /// Contains information about all currently disabled plugins. The key is the BepInEx GUID of the plugin.
    /// </summary>
    public static Dictionary<string, PluginInfo> DisabledPluginInfos { get; } = SpaceWarpPatcher.ChainloaderPatch
        .DisabledPluginGuids.Zip(SpaceWarpPatcher.ChainloaderPatch.DisabledPlugins, (guid, info) => new { guid, info })
        .ToDictionary(item => item.guid, item => item.info);

    /// <summary>
    /// Retrieves the instance of the specified plugin class.
    /// </summary>
    /// <typeparam name="T">The type of the plugin class</typeparam>
    /// <returns>Plugin instance or null if not found</returns>
    public static T TryGetPlugin<T>() where T : BaseUnityPlugin => Chainloader.Plugins.OfType<T>().FirstOrDefault();

    /// <summary>
    /// Retrieves the instance of a plugin class with the given BepInEx GUID.
    /// </summary>
    /// <param name="guid">BepInEx GUID of the plugin</param>
    /// <typeparam name="T">The type of the plugin class</typeparam>
    /// <returns>Plugin instance or null if not found</returns>
    public static T TryGetPlugin<T>(string guid) where T : BaseUnityPlugin =>
        Chainloader.Plugins.Find(plugin => plugin.Info.Metadata.GUID == guid) as T;
}