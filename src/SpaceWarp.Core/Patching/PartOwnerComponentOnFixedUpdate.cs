using System.Runtime.CompilerServices;
using HarmonyLib;
using KSP.Sim.impl;
using SpaceWarp.API.Parts;

namespace SpaceWarp.Patching;

[HarmonyPatch]
internal class PartOwnerComponentOnFixedUpdate
{
    private class ReferenceBoolean
    {
        internal static ReferenceBoolean TRUE = new();
        internal static ReferenceBoolean FALSE = new();
        public static implicit operator ReferenceBoolean(bool b) => b ? TRUE : FALSE;
        public static implicit operator bool(ReferenceBoolean b) => b == TRUE;
    }
    
    private static ConditionalWeakTable<PartOwnerComponent, ReferenceBoolean> AlreadyCachedOwners = new();
    [HarmonyPatch(typeof(PartOwnerComponent), "OnFixedUpdate"), HarmonyPrefix]
    private static bool PerformBackgroundCalculationsForRegisteredModules(double universalTime,
        double deltaUniversalTime,
        PartOwnerComponent __instance)
    {
        if (!AlreadyCachedOwners.TryGetValue(__instance, out var isModulePresent))
        {
            isModulePresent = false;
            var hasPartModuleMethod = __instance.GetType().GetMethod("HasPartModule");

            if (hasPartModuleMethod != null)
            {
                // Go through each registered module and check if it's present in PartOwnerComponent modules
                foreach (var moduleType in PartComponentModuleOverride.RegisteredPartComponentOverrides)
                {
                    var genericMethod = hasPartModuleMethod.MakeGenericMethod(moduleType);
                    if ((bool)genericMethod.Invoke(__instance, null))
                    {
                        isModulePresent = true;
                        break;
                    }
                }
            }

            AlreadyCachedOwners.Add(__instance, isModulePresent);
        }

        // If registered module is present, run the original 0.1.5 method that runs background resource checks
        if (isModulePresent)
        {
            __instance.RecalculatePhysicsStats(false);
            if (__instance.PartAttachmentsDirty)
            {
                __instance.UpdatePartRelationships();
            }
            if (__instance.ResourceFlowRequestManager != null && __instance.FlowGraph != null)
            {
                __instance.ResourceFlowRequestManager.UpdateFlowRequests(universalTime, deltaUniversalTime);
            }
            __instance.UpdateInsolation();
            __instance.UpdateHasPanelsStellarExposure();

            return false;
        }

        // No registered modules are present, resume with the current method 
        return true;
    }
}