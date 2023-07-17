using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using KSP.Game.Flow;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.InternalUtilities;
using SpaceWarp.Patching.LoadingActions;

namespace SpaceWarp.API.Loading;

public static class Loading
{
    internal static List<Func<SpaceWarpPluginDescriptor, DescriptorLoadingAction>> DescriptorLoadingActionGenerators =
        new();

    internal static List<Func<SpaceWarpPluginDescriptor, DescriptorLoadingAction>>
        FallbackDescriptorLoadingActionGenerators =
            new();

    internal static List<Func<BaseSpaceWarpPlugin, ModLoadingAction>> LoadingActionGenerators = new();
    internal static List<Func<FlowAction>> GeneralLoadingActions = new();

    /// <summary>
    /// Registers an asset loading function for space warp, will load assets from the subfolder. Should be added either on Awake() or Start().
    /// </summary>
    /// <param name="subfolder">The subfolder under "assets" that this loader matches</param>
    /// <param name="name">The name to be displayed for this loader, it gets displayed like the following "Mod Name: [name]"</param>
    /// <param name="importFunction">The function used to import an asset, it returns a list of asset names + asset to be added under the mods folder in the AssetManager, its parameters are the internal path to the file and the files true path</param>
    /// <param name="extensions">A list of valid extensions for this action without the ".", leave empty to match any file</param>
    public static void AddAssetLoadingAction(string subfolder, string name,
        Func<string, string, List<(string name, UnityObject asset)>> importFunction, params string[] extensions)
    {
        AddModLoadingAction(name,
            extensions.Length == 0
                ? CreateAssetLoadingActionWithoutExtension(subfolder, importFunction)
                : CreateAssetLoadingActionWithExtensions(subfolder, importFunction, extensions));
        FallbackDescriptorLoadingActionGenerators.Add(d => new DescriptorLoadingAction(name,
            extensions.Length == 0
                ? CreateAssetLoadingActionWithoutExtensionDescriptor(subfolder, importFunction)
                : CreateAssetLoadingActionWithExtensionsDescriptor(subfolder, importFunction, extensions), d));
    }

    /// <summary>
    /// Registers a per mod loading action
    /// </summary>
    /// <param name="name">The name of the action</param>
    /// <param name="action">The action</param>
    [Obsolete("Use AddDescriptorLoadingAction instead")]
    public static void AddModLoadingAction(string name, Action<BaseSpaceWarpPlugin> action)
    {
        LoadingActionGenerators.Add(p => new ModLoadingAction(name, action, p));
    }

    /// <summary>
    /// Registers a per mod loading action (but more general). Should be added either on Awake() or Start().
    /// </summary>
    /// <param name="name">The name of the action</param>
    /// <param name="action">The action</param>
    public static void AddDescriptorLoadingAction(string name, Action<SpaceWarpPluginDescriptor> action)
    {
        DescriptorLoadingActionGenerators.Add(p => new DescriptorLoadingAction(name, action, p));
    }

    /// <summary>
    /// Registers a general loading action. Should be added either on Awake() or Start().
    /// </summary>
    /// <param name="action">The action generator</param>
    public static void AddGeneralLoadingAction(Func<FlowAction> actionGenerator)
    {
        GeneralLoadingActions.Add(actionGenerator);
    }

    /// <summary>
    /// Registers an action to be done on addressables after addressables have been loaded. Should be added either on Awake() or Start().
    /// </summary>
    /// <param name="name">The name of the action</param>
    /// <param name="label">The addressables label to hook into</param>
    /// <param name="action">The action to be done on each addressables asset</param>
    /// <typeparam name="T">The type of asset that this action is done upon</typeparam>
    public static void AddAddressablesLoadingAction<T>(string name, string label, Action<T> action)
        where T : UnityObject
    {
        AddGeneralLoadingAction(() => new AddressableAction<T>(name, label, action));
    }


    private static Action<BaseSpaceWarpPlugin> CreateAssetLoadingActionWithExtensions(string subfolder,
        Func<string, string, List<(string name, UnityObject asset)>> importFunction, string[] extensions)
    {
        return plugin =>
        {
            var path = Path.Combine(plugin.PluginFolderPath, "assets", subfolder);
            if (!Directory.Exists(path)) return;
            var directoryInfo = new DirectoryInfo(path);
            foreach (var extension in extensions)
            {
                foreach (var file in directoryInfo.EnumerateFiles($"*.{extension}", SearchOption.AllDirectories)
                             .Select(fileInfo => fileInfo.FullName))
                {
                    try
                    {
                        LoadSingleAsset(importFunction, path, file, plugin);
                    }
                    catch (Exception e)
                    {
                        plugin.ModLogger.LogError(e.ToString());
                    }
                }
            }
        };
    }

    private static Action<SpaceWarpPluginDescriptor> CreateAssetLoadingActionWithExtensionsDescriptor(string subfolder,
        Func<string, string, List<(string name, UnityObject asset)>> importFunction, string[] extensions)
    {
        return plugin =>
        {
            var path = Path.Combine(plugin.Folder.FullName, "assets", subfolder);
            if (!Directory.Exists(path)) return;
            var directoryInfo = new DirectoryInfo(path);
            foreach (var extension in extensions)
            {
                foreach (var file in directoryInfo.EnumerateFiles($"*.{extension}", SearchOption.AllDirectories)
                             .Select(fileInfo => fileInfo.FullName))
                {
                    try
                    {
                        LoadSingleAsset(importFunction, path, file, plugin);
                    }
                    catch (Exception e)
                    {
                        if (plugin.Plugin != null)
                            plugin.Plugin.SWLogger.LogError(e.ToString());
                        else
                            SpaceWarpPlugin.Logger.LogError(plugin.SWInfo.Name + ": " + e);
                    }
                }
            }
        };
    }

    private static void LoadSingleAsset(Func<string, string, List<(string name, UnityObject asset)>> importFunction,
        string path, string file, BaseSpaceWarpPlugin plugin)
    {
        var assetPathList = PathHelpers.GetRelativePath(path, file)
            .Split(Path.DirectorySeparatorChar);
        var assetPath = "";
        for (var i = 0; i < assetPathList.Length; i++)
        {
            assetPath += assetPathList[i].ToLower();
            if (i != assetPathList.Length - 1)
            {
                assetPath += "/";
            }
        }

        var assets = importFunction(assetPath, file);
        foreach (var asset in assets)
        {
            AssetManager.RegisterSingleAsset(plugin.IdBySpec, asset.name, asset.asset);
        }
    }

    private static void LoadSingleAsset(Func<string, string, List<(string name, UnityObject asset)>> importFunction,
        string path, string file, SpaceWarpPluginDescriptor plugin)
    {
        var assetPathList = PathHelpers.GetRelativePath(path, file)
            .Split(Path.DirectorySeparatorChar);
        var assetPath = "";
        for (var i = 0; i < assetPathList.Length; i++)
        {
            assetPath += assetPathList[i].ToLower();
            if (i != assetPathList.Length - 1)
            {
                assetPath += "/";
            }
        }

        var assets = importFunction(assetPath, file);
        foreach (var asset in assets)
        {
            AssetManager.RegisterSingleAsset(plugin.Guid, asset.name, asset.asset);
        }
    }

    private static Action<BaseSpaceWarpPlugin> CreateAssetLoadingActionWithoutExtension(string subfolder,
        Func<string, string, List<(string name, UnityObject asset)>> importFunction)
    {
        return plugin =>
        {
            var path = Path.Combine(plugin.PluginFolderPath, "assets", subfolder);
            if (!Directory.Exists(path)) return;
            var directoryInfo = new DirectoryInfo(path);
            foreach (var file in directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                         .Select(fileInfo => fileInfo.FullName))
            {
                try
                {
                    LoadSingleAsset(importFunction, path, file, plugin);
                }
                catch (Exception e)
                {
                    plugin.ModLogger.LogError(e.ToString());
                }
            }
        };
    }

    private static Action<SpaceWarpPluginDescriptor> CreateAssetLoadingActionWithoutExtensionDescriptor(
        string subfolder, Func<string, string, List<(string name, UnityObject asset)>> importFunction)
    {
        return plugin =>
        {
            var path = Path.Combine(plugin.Folder.FullName, "assets", subfolder);
            if (!Directory.Exists(path)) return;
            var directoryInfo = new DirectoryInfo(path);
            foreach (var file in directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                         .Select(fileInfo => fileInfo.FullName))
            {
                try
                {
                    LoadSingleAsset(importFunction, path, file, plugin);
                }
                catch (Exception e)
                {
                    if (plugin.Plugin != null)
                        plugin.Plugin.SWLogger.LogError(e.ToString());
                    else
                        SpaceWarpPlugin.Logger.LogError(plugin.SWInfo.Name + ": " + e);
                }
            }
        };
    }
}