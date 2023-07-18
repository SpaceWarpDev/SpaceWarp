using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using KSP.Modding;
using UnityEngine.IO;
using File = System.IO.File;

namespace SpaceWarp.Patching;

[HarmonyPatch(typeof(KSP2Mod))]
public static class ModLoaderPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(KSP2Mod.Load))]
    private static void Load(ref bool __result, ref KSP2Mod __instance)
    {
        if (!__result) return;
        if (__instance.EntryPoint != null)
        {
            // Lets take a simple guess at what needs to be done.
            if (File.Exists(Path.Combine(__instance.ModRootPath, __instance.EntryPoint)))
            {
                try
                {
                    Assembly.LoadFile(Path.Combine(__instance.ModRootPath, __instance.EntryPoint));
                    __instance.modType = KSP2ModType.CSharp;
                }
                catch
                {
                    __result = false;
                }
            }
            else
            {
                __result = false;
            }
        }
        else
        {
            __instance.modType = KSP2ModType.ContentOnly;
        }
    }
}