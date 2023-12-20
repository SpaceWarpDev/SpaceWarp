using HarmonyLib;
using KSP.Sim.impl;
using SpaceWarp.API.Parts;

namespace SpaceWarp.Patching;

[HarmonyPatch]
internal class PartOwnerComponentOnFixedUpdate
{
    [HarmonyPatch(typeof(PartOwnerComponent), "OnFixedUpdate"), HarmonyPrefix]
    private static bool PerformBackgroundCalculationsForRegisteredModules(double universalTime,
        double deltaUniversalTime,
        PartOwnerComponent __instance)
    {
        var isModulePresent = false;

        // Go through each registered module and check if it's present in PartOwnerComponent modules
        foreach (var moduleType in PartComponentModuleOverride.RegisteredPartComponentOverrides)
        {
            var hasPartModuleMethod = __instance.GetType().GetMethod("HasPartModule");

            if (hasPartModuleMethod != null)
            {
                var genericMethod = hasPartModuleMethod.MakeGenericMethod(moduleType);

                if ((bool)genericMethod.Invoke(__instance, null))
                {
                    isModulePresent = true;
                    break;
                }
            }
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