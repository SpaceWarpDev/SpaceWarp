using System;
using System.Reflection;
using HarmonyLib;

namespace SpaceWarp.Patching;

[HarmonyPatch]
internal static class FixGetTypes
{
    [HarmonyFinalizer]
    [HarmonyPatch(typeof(Assembly), nameof(Assembly.GetTypes), new Type[0])]
    [HarmonyPatch(typeof(Assembly), nameof(Assembly.GetExportedTypes))]
    private static Exception GetTypesFix(Exception __exception, Assembly __instance, ref Type[] __result)
    {
        if (__exception is ReflectionTypeLoadException rtle)
        {
            SpaceWarpManager.Logger.LogWarning($"Types failed to load from assembly {__instance.FullName} due to the reasons below, continuing anyway.");
            SpaceWarpManager.Logger.LogWarning($"Exception: {__exception}");
            foreach (var e in rtle.LoaderExceptions)
            {
                SpaceWarpManager.Logger.LogWarning(e.ToString());
            }
            __result = rtle.Types.Where(t => t != null).ToArray();
            return null;
        }
        return __exception;
    }
}
