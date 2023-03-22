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
    private static Exception GetTypesFix(Exception typeException, Assembly instance, ref Type[] result)
    {
        if (typeException is not ReflectionTypeLoadException reflectionTypeLoadException) return typeException;
        SpaceWarpManager.Logger.LogWarning(
            $"Types failed to load from assembly {instance.FullName} due to the reasons below, continuing anyway.");
        SpaceWarpManager.Logger.LogWarning($"Exception: {typeException}");

        foreach (var exception in reflectionTypeLoadException.LoaderExceptions)
            SpaceWarpManager.Logger.LogWarning(exception.ToString());
        result = reflectionTypeLoadException.Types.Where(type => type != null).ToArray();
        return null;

    }
}