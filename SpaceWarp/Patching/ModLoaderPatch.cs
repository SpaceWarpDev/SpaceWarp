using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using KSP.Modding;
using Newtonsoft.Json;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.Backend.Modding;
using SpaceWarp.InternalUtilities;
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
    
    [HarmonyPostfix]
    [HarmonyPatch(nameof(KSP2Mod.Load))]
    private static void Load(ref bool __result, ref KSP2Mod __instance)
    {
        if (!__result) return;
        ISpaceWarpMod swMod = new KspModAdapter(__instance);
        if (__instance.EntryPoint != null)
        {
            // Lets take a simple guess at what needs to be done.
            if (File.Exists(Path.Combine(__instance.ModRootPath, __instance.EntryPoint)))
            {
                if (__instance.EntryPoint.EndsWith(".dll"))
                {
                    try
                    {
                        var asm = Assembly.LoadFile(Path.Combine(__instance.ModRootPath, __instance.EntryPoint));
                        __instance.modType = KSP2ModType.CSharp;
                        foreach (var type in asm.GetTypes())
                        {
                            if (!typeof(Mod).IsAssignableFrom(type) || type.IsAbstract) continue;
                            var go = new GameObject(__instance.ModName);
                            var comp = go.AddComponent(type);
                            if (comp is BaseKspLoaderSpaceWarpMod baseKspLoaderSpaceWarpMod)
                            {
                                swMod = baseKspLoaderSpaceWarpMod;
                            }
                            go.Persist();
                            break;
                        }
                    }
                    catch
                    {
                        __instance.modType = KSP2ModType.Error;
                        __result = false;
                        return;
                    }
                } else if (__instance.EntryPoint.EndsWith(".lua"))
                {
                    try
                    {
                        __instance.modCore =
                            new KSP2LuaModCore(__instance.APIVersion, __instance.ModName, __instance.EntryPoint,
                                __instance.ModRootPath);
                        __instance.modType = KSP2ModType.Lua;
                    }
                    catch
                    {
                        __instance.modType = KSP2ModType.Error;
                        __result = false;
                        return;
                    }
                }
            }
            else
            {
                __instance.modType = KSP2ModType.Error;
                __result = false;
                return;
            }
        }
        else
        {
            __instance.modType = KSP2ModType.ContentOnly;
        }

        var path = __instance.ModRootPath;
        var info = File.Exists(Path.Combine(path, "swinfo.json")) ? JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(path)) : KSPToSwinfo(__instance);
        var descriptor = new SpaceWarpPluginDescriptor(swMod, info.ModID, info.Name, info, new DirectoryInfo(path));
        SpaceWarpManager.AllPlugins.Add(descriptor);
        SpaceWarpManager.InternalModLoaderMods.Add(descriptor);
    }
}