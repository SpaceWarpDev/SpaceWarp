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
        if (__exception is not ReflectionTypeLoadException reflectionTypeLoadException)
        {
            return __exception;
        }

        SpaceWarpManager.Logger.LogWarning(
            $"Types failed to load from assembly {__instance.FullName} due to the reasons below, continuing anyway.");
        SpaceWarpManager.Logger.LogWarning($"Exception: {__exception}");

        foreach (var exception in reflectionTypeLoadException.LoaderExceptions)
        {
            SpaceWarpManager.Logger.LogWarning(exception.ToString());
        }

        __result = reflectionTypeLoadException.Types.Where(type => type != null).ToArray();
        return null;

    }
}