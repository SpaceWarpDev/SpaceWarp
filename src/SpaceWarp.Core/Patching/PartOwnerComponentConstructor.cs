using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using KSP.Sim.impl;
using SpaceWarp.API.Parts;

namespace SpaceWarp.Patching;


[HarmonyPatch(typeof(PartOwnerComponent))]
public static class PartOwnerComponentConstructor
{
    private static FieldInfo AddedField =
        typeof(PartOwnerComponent).GetField("HasRegisteredPartComponentsForFixedUpdate");

    [HarmonyPatch(MethodType.Constructor)]
    public static void SetFalse(PartOwnerComponent __instance)
    {

        AddedField.SetValue(__instance, false);
    }

    [HarmonyPatch(nameof(PartOwnerComponent.Add)), HarmonyPrefix, UsedImplicitly]
    public static void CheckForModule(PartOwnerComponent __instance, PartComponent part)
    {
        var currentValue = (Boolean)AddedField.GetValue(__instance);
        if (currentValue) return;
        var hasModule = PartComponentModuleOverride.RegisteredPartComponentOverrides.Any(type => part.TryGetModule(type, out _));

        AddedField.SetValue(__instance, hasModule);
    }
}