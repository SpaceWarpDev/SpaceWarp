using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using KSP.Game;
using KSP.Modules;
using KSP.OAB;
using SpaceWarp.API.Assets;
using UnityEngine;

namespace SpaceWarp.Patching;

/// <summary>
///     This patch is meant to give modders a way to use the new colors system on KSP2.
///     The patch will replace any renderer that has a "Parts Replace" or a "KSP2/Parts/Paintable" shader on it.
///     It will copy all its values onto the new material, including the material name.
///     Note: "Parts Replace" is obsolete and might be deleted on a later version.
///     Patch created by LuxStice.
/// </summary>
[HarmonyPatch]
internal class ColorsPatch
{
    private const string KSP2_OPAQUE_PATH = "KSP2/Scenery/Standard (Opaque)",
        KSP2_TRANSPARENT_PATH = "KSP2/Scenery/Standard (Transparent)",
        UNITY_STANDARD = "Standard";

    [HarmonyPatch(typeof(ObjectAssemblyPartTracker), nameof(ObjectAssemblyPartTracker.OnPartPrefabLoaded))]
    public static void Prefix(IObjectAssemblyAvailablePart obj, ref GameObject prefab)
    {
        foreach(var renderer in prefab.GetComponentsInChildren<Renderer>())
        {
            string shaderName = renderer.material.shader.name;
            if (shaderName == "Parts Replace" || shaderName == "KSP2/Parts/Paintable")
            {
                var mat = new Material(Shader.Find(KSP2_OPAQUE_PATH));
                mat.name = renderer.material.name;
                mat.CopyPropertiesFromMaterial(renderer.material);
                renderer.material = mat;
            }
        }
    }
}
