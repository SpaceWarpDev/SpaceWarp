using BepInEx;
using BepInEx.Bootstrap;
using ConfigurationManager.Utilities;
using HarmonyLib;

namespace SpaceWarp.Patching.Settings;

[HarmonyPatch]
internal class ConfigurationManagerPatch
{
    [HarmonyPatch(typeof(Utils), nameof(Utils.FindPlugins))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void FindPluginsPatch(ref BaseUnityPlugin[] __result)
    {
        //Disable because I know what I am doing.
        #pragma warning disable CS0618
        __result = Chainloader.Plugins.ToArray();
        #pragma warning restore CS0618
    }
}