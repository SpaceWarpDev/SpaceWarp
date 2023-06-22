using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Bootstrap;
using SpaceWarp.API.Mods.JSON;
using SpaceWarpPatcher;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

// Disable obsolete warning for Chainloader.Plugins
#pragma warning disable CS0618

namespace SpaceWarp.API.Mods;

/// <summary>
/// API for accessing information about currently loaded and disabled plugins.
/// </summary>
public static class PluginList
{
    /// <summary>
    /// Set if the plugin list is different in any way since last run (version differences, new mods, mods removed, mods disabled, description differences, any different in any swinfo file and the disabled mod list)
    /// </summary>
    public static bool ModListChangedSinceLastRun => ChainloaderPatch.ModListChangedSinceLastRun;
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
    /// Returns whether the plugin with the specified GUID is currently loaded.
    /// </summary>
    /// <param name="guid">GUID of the plugin</param>
    /// <returns>Returns true if the plugin is loaded, false otherwise</returns>
    public static bool IsLoaded(string guid)
    {
        return LoadedPluginInfos.ContainsKey(guid);
    }

    /// <summary>
    /// Compares the version of the specified plugin with the given version.
    /// </summary>
    /// <param name="guid">GUID of the plugin</param>
    /// <param name="version">Version to compare the plugin's version to</param>
    /// <returns>Returns -1 if the plugin is older than the given version, 0 if it's the same version and 1 if it's newer</returns>
    /// <exception cref="ArgumentException">Thrown if the plugin with the specified GUID is not loaded</exception>
    public static int CompareVersion(string guid, Version version)
    {
        if (!IsLoaded(guid))
        {
            throw new ArgumentException($"Plugin with GUID {guid} is not loaded");
        }

        return LoadedPluginInfos[guid].Metadata.Version.CompareTo(version);
    }

    /// <summary>
    /// Retrieves the <see cref="ModInfo"/> of the specified plugin. Returns null if the specified plugin guid doesn't
    /// have an associated <see cref="ModInfo"/>.
    /// </summary>
    /// <param name="guid">GUID of the plugin</param>
    /// <returns><see cref="ModInfo"/> of the plugin or null if not found</returns>
    public static ModInfo TryGetSwinfo(string guid)
    {
        var swModInfo = SpaceWarpManager.AllPlugins
            .FirstOrDefault(item => item.Guid == guid);

        if (swModInfo != null)
        {
            return swModInfo.SWInfo;
        }
        
        var disabledModInfo = SpaceWarpManager.DisabledPlugins
            .Where(item => item.Guid == guid)
            .Select(item => item.SWInfo)
            .FirstOrDefault();

        return disabledModInfo;
    }
    /// <summary>
    /// Retrieves the <see cref="SpaceWarpPluginDescriptor"/> of the specified plugin. Returns null if the specified plugin guid doesn't
    /// have an associated <see cref="SpaceWarpPluginDescriptor"/>.
    /// </summary>
    /// <param name="guid">GUID of the plugin</param>
    /// <returns><see cref="SpaceWarpPluginDescriptor"/> of the plugin or null if not found</returns>
    public static SpaceWarpPluginDescriptor TryGetDescriptor(string guid)
    {
        return SpaceWarpManager.AllPlugins
            .FirstOrDefault(item => item.Guid == guid);
    }

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