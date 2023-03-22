using System;
using BepInEx;
using BepInEx.Bootstrap;
using ConfigurationManager.Utilities;
using HarmonyLib;
using JetBrains.Annotations;

namespace SpaceWarp.Patching;

[HarmonyPatch(typeof(Utils))]
[HarmonyPatch(nameof(Utils.FindPlugins))]
public class ConfigurationPatch
{
    static void PostFix(ref BaseUnityPlugin[] __result)
    {
        if (__result == null) throw new ArgumentNullException(nameof(__result));
        //Disable because I know what I am doing.
        #pragma warning disable CS0618
        __result = Chainloader.Plugins.ToArray();
        #pragma warning restore CS0618
    }
}