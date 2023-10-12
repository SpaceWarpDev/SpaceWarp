using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Bootstrap;
using JetBrains.Annotations;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.API.Versions;
using SpaceWarpPatcher;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Switch.LowLevel;

// Disable obsolete warning for Chainloader.Plugins
#pragma warning disable CS0618

namespace SpaceWarp.API.Mods;

/// <summary>
/// API for accessing information about currently loaded and disabled plugins.
/// </summary>
[PublicAPI]
public static class PluginList
{
    #region Reading Plugins

    /// <summary>
    /// Set if the plugin list is different in any way since last run (version differences, new mods, mods removed,
    /// mods disabled, description differences, any different in any swinfo file and the disabled mod list).
    /// </summary>
    public static bool ModListChangedSinceLastRun => ChainloaderPatch.ModListChangedSinceLastRun;

    /// <summary>
    /// Contains information about all currently loaded plugins. The key is the BepInEx GUID of the plugin.
    /// </summary>
    public static Dictionary<string, PluginInfo> LoadedPluginInfos { get; } = Chainloader.PluginInfos;

    /// <summary>
    /// Contains information about all currently disabled plugins. The key is the BepInEx GUID of the plugin.
    /// </summary>
    public static Dictionary<string, PluginInfo> DisabledPluginInfos { get; } = ChainloaderPatch
        .DisabledPluginGuids.Zip(ChainloaderPatch.DisabledPlugins, (guid, info) => new { guid, info })
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
        var swModInfo = AllEnabledAndActivePlugins
            .FirstOrDefault(item => item.Guid == guid);

        if (swModInfo != null)
        {
            return swModInfo.SWInfo;
        }

        var disabledModInfo = AllDisabledPlugins
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
        return AllEnabledAndActivePlugins
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

    #endregion

    #region Registering Plugins

    private static List<SpaceWarpPluginDescriptor> _allEnabledAndActivePlugins = new();

    /// <summary>
    /// All plugins that are enabled, and active (not errored)
    /// </summary>
    public static IReadOnlyList<SpaceWarpPluginDescriptor> AllEnabledAndActivePlugins => _allEnabledAndActivePlugins;

    private static List<SpaceWarpPluginDescriptor> _allDisabledPlugins = new();

    /// <summary>
    /// All disabled plugins
    /// </summary>
    public static IReadOnlyList<SpaceWarpPluginDescriptor> AllDisabledPlugins => _allDisabledPlugins;

    private static List<SpaceWarpErrorDescription> _allErroredPlugins = new();
    public static IReadOnlyList<SpaceWarpErrorDescription> AllErroredPlugins => _allErroredPlugins;

    public static IEnumerable<SpaceWarpPluginDescriptor> AllPlugins => _allEnabledAndActivePlugins
        .Concat(_allDisabledPlugins).Concat(_allErroredPlugins.Select(x => x.Plugin));

    public static void RegisterPlugin(SpaceWarpPluginDescriptor plugin)
    {
        if (AllPlugins.Any(x => x.Guid == plugin.Guid))
        {
            SpaceWarpPlugin.Logger.LogError($"Attempting to register a mod with a duplicate GUID: {plugin.Guid}");
        }

        SpaceWarpPlugin.Logger.LogInfo($"Registered plugin: {plugin.Guid}");
        _allEnabledAndActivePlugins.Add(plugin);
    }

    public static void Disable(string guid)
    {
        var descriptor = _allEnabledAndActivePlugins.FirstOrDefault(x =>
            string.Equals(x.Guid, guid, StringComparison.InvariantCultureIgnoreCase));
        if (descriptor != null)
        {
            _allEnabledAndActivePlugins.Remove(descriptor);
            _allDisabledPlugins.Add(descriptor);
        }
    }

    public static SpaceWarpErrorDescription GetErrorDescriptor(SpaceWarpPluginDescriptor plugin)
    {
        
        if (_allErroredPlugins.Any(x => x.Plugin == plugin))
        {
            return _allErroredPlugins.First(x => x.Plugin == plugin);
        }

        if (_allEnabledAndActivePlugins.Any(x => x == plugin))
        {
            _allEnabledAndActivePlugins.Remove(plugin);
        }

        var newError = new SpaceWarpErrorDescription(plugin);
        _allErroredPlugins.Add(newError);
        return newError;
    }

    public static void NoteMissingSwinfoError(SpaceWarpPluginDescriptor plugin)
    {
        var errorDescriptor = GetErrorDescriptor(plugin);
        errorDescriptor.MissingSwinfo = true;
    }

    public static void NoteBadDirectoryError(SpaceWarpPluginDescriptor plugin)
    {
        var errorDescriptor = GetErrorDescriptor(plugin);
        errorDescriptor.BadDirectory = true;
    }

    public static void NoteBadIDError(SpaceWarpPluginDescriptor plugin)
    {
        var errorDescriptor = GetErrorDescriptor(plugin);
        errorDescriptor.BadID = true;
    }

    public static void NoteMismatchedVersionError(SpaceWarpPluginDescriptor plugin)
    {
        var errorDescriptor = GetErrorDescriptor(plugin);
        errorDescriptor.MismatchedVersion = true;
    }

    public static void NoteUnspecifiedDependencyError(SpaceWarpPluginDescriptor plugin, string dependency)
    {
        var errorDescriptor = GetErrorDescriptor(plugin);
        errorDescriptor.UnspecifiedDependencies.Add(dependency);
    }

    private static SemanticVersion PadVersion(string version)
    {
        var length = version.Split('.').Length;
        for (var i = 0; i < 3-length; i++)
        {
            version += ".0";
        }

        return new SemanticVersion(version);
    }

    private static bool IsSupportedSemver(string version, string min, string max)
    {
        var basicVersion = PadVersion(version);
        var minVersion = PadVersion(min.Replace("*", "0"));
        var maxVersion = PadVersion(max.Replace("*", $"{int.MaxValue}"));
        return basicVersion >= minVersion && basicVersion <= maxVersion;
    }
    
    private static bool DependencyResolved(
        SpaceWarpPluginDescriptor descriptor,
        List<SpaceWarpPluginDescriptor> resolvedPlugins
    )
    {
        if (descriptor.SWInfo.Spec < SpecVersion.V1_3) return true;
        return !(from dependency in descriptor.SWInfo.Dependencies
            let info = resolvedPlugins.FirstOrDefault(x => string.Equals(
                x.Guid,
                dependency.ID,
                StringComparison.InvariantCultureIgnoreCase)
            )
            where info == null || !IsSupportedSemver(
                info.SWInfo.Version,
                dependency.Version.Min,
                dependency.Version.Max
            )
            select dependency).Any();
    }

    private static void GetLoadOrder()
    {
        var changed = true;
        List<SpaceWarpPluginDescriptor> newOrder = new();
        while (changed)
        {
            changed = false;
            for (var i = _allEnabledAndActivePlugins.Count - 1; i >= 0; i--)
            {
                if (!DependencyResolved(_allEnabledAndActivePlugins[i], newOrder)) continue;
                newOrder.Add(_allEnabledAndActivePlugins[i]);
                _allEnabledAndActivePlugins.RemoveAt(i);
                changed = true;
            }
        }

        for (var i = _allEnabledAndActivePlugins.Count - 1; i >= 0; i--)
        {
            var info = _allEnabledAndActivePlugins[i];
            SpaceWarpPlugin.Logger.LogError($"Missing dependency for mod: {info.Name}, this mod will not be loaded");
            var error = GetErrorDescriptor(info);
            error.MissingDependencies = info.SWInfo.Dependencies.Select(x => x.ID).Where(x =>
                !newOrder.Any(y => string.Equals(x, y.Guid, StringComparison.InvariantCultureIgnoreCase))).ToList();
        }

        _allEnabledAndActivePlugins = newOrder;
    }

    private static void GetDependencyErrors()
    {
        foreach (var erroredPlugin in _allErroredPlugins.Where(erroredPlugin =>
                     erroredPlugin.MissingDependencies.Count != 0))
        {
            for (var i = erroredPlugin.MissingDependencies.Count - 1; i >= 0; i--)
            {
                var dep = erroredPlugin.MissingDependencies[i];
                if (AllEnabledAndActivePlugins.Any(x =>
                        string.Equals(x.Guid, dep, StringComparison.InvariantCultureIgnoreCase)))
                {
                    erroredPlugin.UnsupportedDependencies.Add(dep);
                    erroredPlugin.MissingDependencies.RemoveAt(i);
                }
                else if (AllErroredPlugins.Any(x =>
                             string.Equals(x.Plugin.Guid, dep, StringComparison.InvariantCultureIgnoreCase)))
                {
                    erroredPlugin.ErroredDependencies.Add(dep);
                    erroredPlugin.MissingDependencies.RemoveAt(i);
                }
                else if (AllDisabledPlugins.Any(x =>
                             string.Equals(x.Guid, dep, StringComparison.InvariantCultureIgnoreCase)))
                {
                    erroredPlugin.DisabledDependencies.Add(dep);
                    erroredPlugin.MissingDependencies.RemoveAt(i);
                }
            }
        }
    }

    private static void CheckCompatibility()
    {
        var incompatibilities = _allEnabledAndActivePlugins.Select(x => (Key: x.Guid, Value: new HashSet<string>()))
            .ToDictionary(x => x.Key, x => x.Value);
        var versionLookup = _allEnabledAndActivePlugins.Select(x => (Key: x.Guid, Value: x.SWInfo.Version))
            .ToDictionary(x => x.Key, x => x.Value);
        var pluginDictionary = _allEnabledAndActivePlugins.ToDictionary(x => x.Guid, x => x);
        foreach (var mod in _allEnabledAndActivePlugins)
        {
            var swinfo = mod.SWInfo;
            if (swinfo.Spec < SpecVersion.V2_0) continue;
            foreach (var conflict in swinfo.Conflicts)
            {
                if (!versionLookup.TryGetValue(conflict.ID, out var conflictingVersion) ||
                    !IsSupportedSemver(conflictingVersion, conflict.Version.Min, conflict.Version.Max)) continue;
                incompatibilities[mod.Guid].Add(conflict.ID);
                incompatibilities[conflict.ID].Add(mod.Guid);
            }
        }

        foreach (var incompatibility in incompatibilities)
        {
            if (incompatibility.Value.Count <= 0) continue;
            var descriptor = GetErrorDescriptor(pluginDictionary[incompatibility.Key]);
            descriptor.Incompatibilities.AddRange(incompatibility.Value);
        }
    }

    /// <summary>
    /// This is done after Awake/LoadModule(), so that everything else can use it
    /// </summary>
    internal static void ResolveDependenciesAndLoadOrder()
    {
        GetLoadOrder();
        GetDependencyErrors();
        CheckCompatibility();
    }

    #endregion
}