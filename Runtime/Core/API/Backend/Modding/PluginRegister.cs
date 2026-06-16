using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using ReduxLib.Configuration;
using ReduxLib.Logging;
using SpaceWarp2.API.Mods;
using SpaceWarp2.API.Mods.JSON;
using UnityEngine;
namespace SpaceWarp2.API.Backend.Modding;

internal static class PluginRegister
{
    public static void RegisterAllMods()
    {
        RegisterSpaceWarp();
        RegisterInternalMods();
        RegisterKsp1Mods();
        RegisterMods();
        RegisterStandaloneLuaMods();
        DisableMods();
    }

    /// <summary>
    /// Registers a descriptor for each standalone drop-in .lua under the mods folder - a .lua with no swinfo
    /// mod folder around it. These are mods-of-one whose ModId is the filename, matching PatchManager's
    /// LoadSinglePatchFile convention, so the SW lifecycle global can resolve their descriptor by ModId.
    /// </summary>
    private static void RegisterStandaloneLuaMods()
    {
        var modsFolder = new DirectoryInfo(CommonPaths.ModsFolder);
        if (!modsFolder.Exists)
        {
            return;
        }

        foreach (var lua in modsFolder.GetFiles("*.lua", SearchOption.AllDirectories))
        {
            // A .lua inside a swinfo mod folder belongs to that mod, not its own mod-of-one.
            if (HasSwinfoAncestor(lua.Directory, modsFolder))
            {
                continue;
            }

            var modId = Path.GetFileNameWithoutExtension(lua.Name);
            if (ModList.DisabledPluginGuids.Contains(modId) || PluginList.TryGetDescriptor(modId) != null)
            {
                continue;
            }

            ISpaceWarpMod swMod = new AssetOnlyMod(modId);
            var info = new ModInfo
            {
                Spec = SpecVersion.Default,
                ModID = modId,
                Name = modId,
                Version = "0.0.0"
            };
            var descriptor = new SpaceWarpPluginDescriptor(
                swMod, modId, modId, info, lua.Directory!, true, swMod.SWConfiguration);
            swMod.SWMetadata = descriptor;
            descriptor.ScriptFiles.Add(lua.FullName);
            PluginList.RegisterPlugin(descriptor);
            Logger.LogInfo($"Registered standalone Lua mod: {modId}");
        }
    }

    /// <summary>
    /// Returns true if <paramref name="dir" /> or any ancestor up to <paramref name="stopAt" /> contains a
    /// swinfo.json - meaning the file belongs to that swinfo mod rather than being a standalone drop-in.
    /// </summary>
    private static bool HasSwinfoAncestor(DirectoryInfo dir, DirectoryInfo stopAt)
    {
        for (var current = dir; current != null; current = current.Parent)
        {
            if (File.Exists(Path.Combine(current.FullName, "swinfo.json")))
            {
                return true;
            }

            if (string.Equals(current.FullName, stopAt.FullName, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
        }

        return false;
    }

    private static void RegisterKsp1Mods()
    {
        foreach (var mod in IInternalModRegister.Instance.Ksp1PluginDescriptors)
        {
            mod.IsKsp1 = true;
            if (mod.Plugin != null)
            {
                mod.Plugin.SWLogger ??= ReduxLib.ReduxLib.GetLogger(mod.Guid);
            }
            PluginList.RegisterPlugin(mod);
        }
    }

    private static void RegisterInternalMods()
    {
        foreach (var mod in IInternalModRegister.Instance.InternalPluginDescriptors)
        {
            mod.Plugin.SWLogger ??= ReduxLib.ReduxLib.GetLogger(mod.Guid);
            mod.Plugin.SWConfiguration = mod.ConfigFile = new JsonConfigFile(Path.Combine(mod.Folder.FullName, "config.json"));
            mod.IsCore = true;
            PluginList.RegisterPlugin(mod);
        }
    }


    private static readonly ReduxLib.Logging.ILogger Logger = SpaceWarpPlugin.Logger;

    private static bool AssertFolderPath(ISpaceWarpMod plugin, string folderPath)
    {
        if (Path.GetFileName(folderPath) != "mods") return true;

        Logger.LogError(
            $"Found Space Warp mod in the BepInEx/plugins directory. This mod will " +
            $"not be initialized."
        );

        return false;
    }

    private static bool AssertModInfoExistence(ISpaceWarpMod plugin, string modInfoPath, string folderPath)
    {
        if (File.Exists(modInfoPath))
        {
            return true;
        }

        Logger.LogError(
            $"Found Space Warp plugin at {modInfoPath} without a swinfo.json in its folder. This mod " +
            $"will not be initialized."
        );

        PluginList.NoteMissingSwinfoError(new SpaceWarpPluginDescriptor(plugin, "unknown", Path.GetFileName(folderPath),
            new ModInfo(), new DirectoryInfo(folderPath)));

        return false;
    }

    private static bool TryReadModInfo(
        ISpaceWarpMod plugin,
        string modInfoPath,
        string folderPath,
        out ModInfo metadata
    )
    {
        try
        {
            metadata = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(modInfoPath));
        }
        catch
        {
            Logger.LogError(
                $"Error reading metadata for spacewarp plugin at {folderPath}. This mod will not be initialized");
            PluginList.NoteMissingSwinfoError(new SpaceWarpPluginDescriptor(plugin, "unknown", Path.GetFileName(folderPath),
                new ModInfo(), new DirectoryInfo(folderPath)));
            metadata = null;
            return false;
        }

        return true;
    }

    private static void RegisterSpaceWarp()
    {
        var mod = new UnloadedMod(typeof(SpaceWarpPlugin))
        {
            SWLogger = SpaceWarpPlugin.Logger,
            SWConfiguration = ReduxLib.ReduxLib.ReduxCoreConfig
        };
        var descriptor = new SpaceWarpPluginDescriptor(mod,
            SpaceWarpPlugin.SpaceWarpModInfo.ModID, SpaceWarpPlugin.SpaceWarpModInfo.Name,
            SpaceWarpPlugin.SpaceWarpModInfo, new DirectoryInfo(ReduxLib.ReduxLib.REDUX_FOLDER), true, ReduxLib.ReduxLib.ReduxCoreConfig);
        mod.SWMetadata = descriptor;
        descriptor.IsCore = true;
        PluginList.RegisterPlugin(descriptor);
    }

    private static void RegisterMods()
    {
        var pluginPath = new DirectoryInfo(CommonPaths.ModsFolder);
        if (!pluginPath.Exists)
        {
            pluginPath.Create();
        }
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
                    $"Found swinfo information for: {swinfoData.Name}, but its spec is less than 1.3, as such this " +
                    $"mod will be ignored"
                );
                continue;
            }

            var modAssemblies = new List<Assembly>();

            // Load the libraries as we get them
            if (Directory.Exists(Path.Combine(swinfo.Directory!.FullName, "lib")) && !ModList.DisabledPluginGuids.Contains(swinfoData.ModID))
            {
                var dirInfo = new DirectoryInfo(Path.Combine(swinfo.Directory!.FullName, "lib"));
                foreach (var dll in dirInfo.GetFiles("*.dll", SearchOption.AllDirectories))
                {
                    modAssemblies.Add(Assembly.LoadFile(dll.FullName));
                }
            }

            // But then load the 
            ISpaceWarpMod swMod = new AssetOnlyMod(swinfoData.Name);
            if (swinfoData.MainAssembly != null && !ModList.DisabledPluginGuids.Contains(swinfoData.ModID))
            {
                Assembly asm = null;
                if (Application.isEditor)
                {
                    // The mod assembly may be compiled by unity and should already be in the loaded app domain.
                    var shortName = Path.GetFileNameWithoutExtension(swinfoData.MainAssembly);
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        var thisShortName = assembly.GetName().Name;
                        if (shortName == thisShortName)
                        {
                            asm = assembly;
                            break;
                        }
                    }
                }
                else
                {
                    var dll = Path.Combine(swinfo.Directory!.FullName, swinfoData.MainAssembly);
                    if (!File.Exists(dll))
                    {
                        // TODO: Add a bad assembly error to the mods list
                        Logger.LogError(
                            $"Main assembly {swinfoData.MainAssembly} for {swinfoData.Name} could not be found, this mod will be ignored");
                        continue;
                    }

                    asm = Assembly.LoadFile(dll);
                }

                if (asm is null)
                {
                    Debug.LogWarning($"Can't find main assembly {swinfoData.MainAssembly} for mod {swinfoData.Name}");
                    continue;
                }

                modAssemblies.Add(asm);

                foreach (var type in asm.GetTypes())
                {
                    if (!typeof(ISpaceWarpMod).IsAssignableFrom(type) || type.IsAbstract) continue;
                    swMod = new UnloadedMod(type);
                    break;
                }
            }

            swMod.SWLogger = ReduxLib.ReduxLib.GetLogger(swinfoData.ModID);

            swMod.SWConfiguration =
                new JsonConfigFile(Path.Combine(swinfo.Directory.FullName, swinfoData.ModID + "-config.json"));

            var descriptor = new SpaceWarpPluginDescriptor(
                swMod,
                swinfoData.ModID,
                swinfoData.Name,
                swinfoData, swinfo.Directory,
                true,
                swMod.SWConfiguration
            );
            swMod.SWMetadata = descriptor;
            descriptor.Assemblies.AddRange(modAssemblies);
            descriptor.ScriptFiles.AddRange(Directory
                .GetFiles(swinfo.Directory!.FullName, "*.lua", SearchOption.AllDirectories)
                .Where(f => !Path.GetFileName(f).StartsWith("_")));

            Logger.LogInfo($"Attempting to register mod: {swinfoData.ModID}, {swinfoData.Name}");

            if (PluginList.AllPlugins.Any(
                    x => string.Equals(x.Guid, swinfoData.ModID, StringComparison.InvariantCultureIgnoreCase)
                ))
            {
                continue;
            }

            // Now we can just add it to our plugin list
            PluginList.RegisterPlugin(descriptor);
        }
    }

    private static void DisableMods()
    {
        foreach (var mod in ModList.DisabledPluginGuids)
        {
            PluginList.Disable(mod);
        }
    }
}