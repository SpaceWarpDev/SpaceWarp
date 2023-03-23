using System;
using BepInEx;
using BepInEx.Bootstrap;
using ConfigurationManager.Utilities;
using HarmonyLib;

namespace SpaceWarp.Patching;

[HarmonyPatch(typeof(Utils))]
[HarmonyPatch(nameof(Utils.FindPlugins))]
public class ConfigurationPatch
{
    static void Postfix(ref BaseUnityPlugin[] __result)
    {
        //Disable because I know what I am doing.
        #pragma warning disable CS0618
        __result = Chainloader.Plugins.ToArray();
        #pragma warning restore CS0618
    }
}