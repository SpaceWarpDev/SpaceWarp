using System;
using System.Collections.Generic;
using System.IO;
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
using SpaceWarp.UI.ModList;
using SpaceWarpPatcher;
using UnityEngine;
using UnityEngine.IO;
using File = System.IO.File;

namespace SpaceWarp.Patching;

[HarmonyPatch(typeof(KSP2Mod))]
public static class ModLoaderPatch
{
    private static ModInfo KSPToSwinfo(KSP2Mod mod)
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
            VersionCheck = null,
            VersionCheckType = VersionCheckType.SwInfo
        };
        return newInfo;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(KSP2Mod.Load))]
    private static bool LoadPre(KSP2Mod __instance, ref bool __result)
    {
        SpaceWarpPlugin.Logger.LogInfo($"KSP2Mod.Load (Pre): {__instance.ModName}");
        var disabled = ChainloaderPatch.DisabledPluginGuids.Contains(__instance.ModName);
        if (!disabled) return true;
        var path = __instance.ModRootPath;
        var info = File.Exists(Path.Combine(path, "swinfo.json"))
            ? JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(path))
            : KSPToSwinfo(__instance);
        var descriptor = new SpaceWarpPluginDescriptor(null, info.ModID, info.Name, info, new DirectoryInfo(path));
        SpaceWarpManager.DisabledPlugins.Add(descriptor);
        SpaceWarpManager.PluginGuidEnabledStatus.Add((descriptor.Guid, false));
        __result = false;
        __instance.currentState = KSP2ModState.Disabled;
        return false;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(KSP2Mod.Load))]
    private static void LoadPost( KSP2Mod __instance, ref bool __result)
    {
        SpaceWarpPlugin.Logger.LogInfo($"KSP2Mod.Load (Post): {__instance.ModName}");
        var disabled = ChainloaderPatch.DisabledPluginGuids.Contains(__instance.ModName);
        if (disabled) return;
        // ISpaceWarpMod swMod = new KspModAdapter(__instance);
        ISpaceWarpMod swMod = null;
        IConfigFile configFile = null;
        var go = new GameObject(__instance.ModName);
        go.Persist();
        var addAdapter = true;
        if (__instance.EntryPoint != null)
        {
            if (LoadCodeBasedMod(__instance, ref __result, go, ref swMod, ref addAdapter, ref configFile)) return;
        }
        else
        {
            __instance.modType = KSP2ModType.ContentOnly;
        }

        if (addAdapter)
        {
            var adapter = go.AddComponent<KspModAdapter>();
            adapter.AdaptedMod = __instance;
            swMod = adapter;
        }

        go.SetActive(true);
        var path = __instance.ModRootPath;
        var info = File.Exists(Path.Combine(path, "swinfo.json"))
            ? JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(path))
            : KSPToSwinfo(__instance);
        var descriptor = new SpaceWarpPluginDescriptor(swMod, info.ModID, info.Name, info, new DirectoryInfo(path), configFile);
        SpaceWarpManager.AllPluginsInternal.Add(descriptor);
        SpaceWarpManager.InternalModLoaderMods.Add(descriptor);
        SpaceWarpManager.PluginGuidEnabledStatus.Add((descriptor.Guid, true));
    }

    private static bool LoadCodeBasedMod(KSP2Mod __instance, ref bool __result, GameObject go, ref ISpaceWarpMod swMod,
        ref bool addAdapter, ref IConfigFile configFile)
    {
        // Lets take a simple guess at what needs to be done.
        if (File.Exists(Path.Combine(__instance.ModRootPath, __instance.EntryPoint)))
        {
            if (__instance.EntryPoint.EndsWith(".dll"))
            {
                if (LoadModWithDLLEntryPoint(__instance, ref __result, go, ref swMod, ref addAdapter, ref configFile)) return true;
            }
            else if (__instance.EntryPoint.EndsWith(".lua"))
            {
                if (LoadModWithLuaEntryPoint(__instance, ref __result)) return true;
            }
        }
        else
        {
            __instance.modType = KSP2ModType.Error;
            __result = false;
            return true;
        }

        return false;
    }

    private static bool LoadModWithLuaEntryPoint(KSP2Mod __instance, ref bool __result)
    {
        try
        {
            __instance.modCore =
                new KSP2LuaModCore(__instance.APIVersion, __instance.ModName, __instance.EntryPoint,
                    __instance.ModRootPath);
            __instance.modType = KSP2ModType.Lua;
            __instance.currentState = KSP2ModState.Active;
        }
        catch (Exception e)
        {
            SpaceWarpPlugin.Logger.LogError(e);
            __instance.modType = KSP2ModType.Error;
            __result = false;
            return true;
        }

        return false;
    }

    private static bool LoadModWithDLLEntryPoint(KSP2Mod __instance, ref bool __result, GameObject go,
        ref ISpaceWarpMod swMod, ref bool addAdapter, ref IConfigFile configFile)
    {
        try
        {
            var asm = Assembly.LoadFile(Path.Combine(__instance.ModRootPath, __instance.EntryPoint));
            __instance.modType = KSP2ModType.CSharp;
            foreach (var type in asm.GetTypes())
            {
                if (!typeof(Mod).IsAssignableFrom(type) || type.IsAbstract) continue;
                var comp = go.AddComponent(type);
                if (comp is BaseKspLoaderSpaceWarpMod baseKspLoaderSpaceWarpMod)
                {
                    baseKspLoaderSpaceWarpMod.SWLogger = new UnityLogSource(__instance.ModName);
                    // baseKspLoaderSpaceWarpMod.modFolder = __instance.ModRootPath;
                    configFile = baseKspLoaderSpaceWarpMod.SWConfiguration =
                        new JsonConfigFile(Path.Combine(__instance.ModRootPath, "config.cfg"));
                    swMod = baseKspLoaderSpaceWarpMod;
                    addAdapter = false;
                }

                break;
            }

            __instance.currentState = KSP2ModState.Active;
        }
        catch (Exception e)
        {
            SpaceWarpPlugin.Logger.LogError(e);
            __instance.modType = KSP2ModType.Error;
            __result = false;
            return true;
        }

        return false;
    }
}