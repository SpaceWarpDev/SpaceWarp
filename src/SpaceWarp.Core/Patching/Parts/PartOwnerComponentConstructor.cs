using System.Reflection;
using HarmonyLib;
using KSP.Sim.impl;
using SpaceWarp.API.Parts;

namespace SpaceWarp.Patching.Parts;

[HarmonyPatch(typeof(PartOwnerComponent))]
internal static class PartOwnerComponentConstructor
{
    private static readonly FieldInfo AddedField =
        typeof(PartOwnerComponent).GetField("HasRegisteredPartComponentsForFixedUpdate");

    [HarmonyPatch(MethodType.Constructor)]
    // ReSharper disable once InconsistentNaming
    private static void SetFalse(PartOwnerComponent __instance)
    {
        AddedField.SetValue(__instance, false);
    }

    [HarmonyPatch(nameof(PartOwnerComponent.Add))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static void CheckForModule(PartOwnerComponent __instance, PartComponent part)
    {
        var currentValue = (bool)AddedField.GetValue(__instance);
        if (currentValue)
        {
            return;
        }

        var hasModule = PartComponentModuleOverride.RegisteredPartComponentOverrides.Any(
            type => part.TryGetModule(type, out _)
        );

        AddedField.SetValue(__instance, hasModule);
    }
}