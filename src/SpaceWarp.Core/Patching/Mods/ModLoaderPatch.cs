using System.Reflection;
using HarmonyLib;
using KSP.Modding;
using Newtonsoft.Json;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Logging;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.Backend.Modding;
using SpaceWarp.InternalUtilities;
using SpaceWarp.Patcher;
using SpaceWarp.Patcher.API;
using UnityEngine;
using File = System.IO.File;

namespace SpaceWarp.Patching.Mods;

[HarmonyPatch(typeof(KSP2Mod))]
internal static class ModLoaderPatch
{
    private static ModInfo KspToSwinfo(KSP2Mod mod)
    {
        var newInfo = new ModInfo
        {
            Spec = SpecVersion.V1_3,
            ModID = mod.ModName,
            Name = mod.ModName,
            Author = mod.ModAuthor,
            Description = mod.ModDescription,
            Source = "<unknown>",
            Version = mod.ModVersion.ToString(),
            Dependencies = new List<DependencyInfo>(),
            SupportedKsp2Versions = new SupportedVersionsInfo
            {
                Min = "*",
                Max = "*"
            },
            VersionCheck = null
        };
        return newInfo;
    }


    [HarmonyPatch(nameof(KSP2Mod.Load))]
    [HarmonyPrefix]
    // ReSharper disable InconsistentNaming
    private static bool LoadPre(KSP2Mod __instance, ref bool __result, out bool __state)
    {
        // ReSharper restore InconsistentNaming
        SpaceWarpPlugin.Instance.SWLogger.LogInfo($"KSP2Mod.Load (Pre): {__instance.ModName}");
        var path = __instance.ModRootPath;
        var info = File.Exists(Path.Combine(path, "swinfo.json"))
            ? JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(path))
            : KspToSwinfo(__instance);
        var disabled = ModList.DisabledPluginGuids.Contains(info.ModID);
        __state = disabled;
        return !disabled;
    }

    [HarmonyPatch(nameof(KSP2Mod.Load))]
    [HarmonyPostfix]
    // ReSharper disable InconsistentNaming
    private static void LoadPost(KSP2Mod __instance, ref bool __result, ref bool __state)
    {
        // ReSharper restore InconsistentNaming
        SpaceWarpPlugin.Instance.SWLogger.LogInfo($"KSP2Mod.Load (Post): {__instance.ModName}");
        if (__state) return;
        var path = __instance.ModRootPath;
        var info = File.Exists(Path.Combine(path, "swinfo.json"))
            ? JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(path))
            : KspToSwinfo(__instance);
        var descriptor =
            PluginList.AllPlugins.FirstOrDefault(x =>
                string.Equals(x.Guid, info.ModID, StringComparison.InvariantCultureIgnoreCase));
        if (descriptor == null) return;
        var go = new GameObject(__instance.ModName);
        go.Persist();
        var addAdapter = true;
        if (__instance.EntryPoint != null)
        {
            if (LoadCodeBasedMod(__instance, ref __result, go, ref descriptor.Plugin, ref addAdapter,
                    ref descriptor.ConfigFile, ref descriptor.DoLoadingActions))
            {
                return;
            }
        }
        else
        {
            __instance.modType = KSP2ModType.ContentOnly;
        }

        SpaceWarpPlugin.Instance.SWLogger.LogInfo($"KSP2Mod.Load (Loaded stuff): {__instance.ModName}");

        if (addAdapter)
        {
            var adapter = go.AddComponent<KspModAdapter>();
            adapter.AdaptedMod = __instance;
            descriptor.Plugin = adapter;
        }

        descriptor.Plugin!.SWMetadata = descriptor;

        go.SetActive(true);
        // var descriptor = new SpaceWarpPluginDescriptor(swMod, info.ModID, info.Name, info, new DirectoryInfo(path), configFile);
    }

    private static bool LoadCodeBasedMod(
        KSP2Mod instance,
        ref bool result,
        GameObject go,
        ref ISpaceWarpMod swMod,
        ref bool addAdapter,
        ref IConfigFile configFile,
        ref bool isSWMod
    )
    {
        // Lets take a simple guess at what needs to be done.
        if (File.Exists(Path.Combine(instance.ModRootPath, instance.EntryPoint)))
        {
            if (instance.EntryPoint.EndsWith(".dll"))
            {
                if (LoadModWithDLLEntryPoint(
                        instance,
                        ref result,
                        go,
                        ref swMod,
                        ref addAdapter,
                        ref configFile,
                        ref isSWMod
                    ))
                {
                    return true;
                }
            }
            else if (instance.EntryPoint.EndsWith(".lua"))
            {
                if (LoadModWithLuaEntryPoint(instance, ref result))
                {
                    return true;
                }
            }
        }
        else
        {
            instance.modType = KSP2ModType.Error;
            result = false;
            return true;
        }

        return false;
    }

    private static bool LoadModWithLuaEntryPoint(KSP2Mod instance, ref bool result)
    {
        try
        {
            instance.modCore = new KSP2LuaModCore(
                instance.APIVersion,
                instance.ModName,
                instance.EntryPoint,
                instance.ModRootPath
            );
            instance.modType = KSP2ModType.Lua;
            instance.currentState = KSP2ModState.Active;
        }
        catch (Exception e)
        {
            SpaceWarpPlugin.Instance.SWLogger.LogError(e);
            instance.modType = KSP2ModType.Error;
            result = false;
            return true;
        }

        return false;
    }

    private static bool LoadModWithDLLEntryPoint(
        KSP2Mod instance,
        ref bool result,
        GameObject go,
        ref ISpaceWarpMod swMod,
        ref bool addAdapter,
        ref IConfigFile configFile,
        ref bool isSWMod
    )
    {
        try
        {
            var asm = Assembly.LoadFile(Path.Combine(instance.ModRootPath, instance.EntryPoint));
            instance.modType = KSP2ModType.CSharp;
            foreach (var type in asm.GetTypes())
            {
                if (!typeof(Mod).IsAssignableFrom(type) || type.IsAbstract)
                {
                    continue;
                }

                var comp = go.AddComponent(type);
                if (comp is not BaseKspLoaderSpaceWarpMod baseKspLoaderSpaceWarpMod)
                {
                    continue;
                }

                SpaceWarpPlugin.Instance.SWLogger.LogInfo($"Loading mod: {comp}");
                isSWMod = true;
                baseKspLoaderSpaceWarpMod.SWLogger = new UnityLogSource(instance.ModName);
                // baseKspLoaderSpaceWarpMod.modFolder = __instance.ModRootPath;
                configFile = baseKspLoaderSpaceWarpMod.SWConfiguration = new JsonConfigFile(
                    Path.Combine(instance.ModRootPath, "config.cfg")
                );
                swMod = baseKspLoaderSpaceWarpMod;
                addAdapter = false;
            }

            instance.currentState = KSP2ModState.Active;
        }
        catch (Exception e)
        {
            SpaceWarpPlugin.Instance.SWLogger.LogError(e);
            instance.modType = KSP2ModType.Error;
            result = false;
            return true;
        }

        return false;
    }
}