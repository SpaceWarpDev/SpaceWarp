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
    private static void Postfix([NotNull] ref BaseUnityPlugin[] result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));
        //Disable because I know what I am doing.
        #pragma warning disable CS0618
        result = Chainloader.Plugins.ToArray();
        #pragma warning restore CS0618
    }
}