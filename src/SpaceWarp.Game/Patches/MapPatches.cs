using HarmonyLib;
using KSP.Map;
using KSP.Sim;
using KSP.Sim.impl;
using UnityEngine;

namespace SpaceWarp.Patches;

[HarmonyPatch]
internal static class MapPatches
{
    [HarmonyPatch(typeof(Map3DView), nameof(Map3DView.ProcessSingleMapItem))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    public static bool SkipBadItems(Map3DView __instance, MapItem item) =>
        item.MapItemType != MapItemType.Waypoint ||
        ((ISimulationModelMap)__instance.Game.UniverseModel).FromGlobalId(item.SimGUID) != null;
}