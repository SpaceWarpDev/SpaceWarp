using HarmonyLib;
using SpaceWarp.API.UI;
using UnityEngine;

namespace SpaceWarp.Patching;
[HarmonyPatch(typeof(ConfigurationManager.ConfigurationManager))]
[HarmonyPatch("OnGUI")]
public class ConfigurationManagerPatch
{
    public static void Prefix()
    {
        GUI.skin = Skins.ConsoleSkin;
    }
}