using System.Collections.Generic;
using HarmonyLib;
using KSP.UI;

namespace SpaceWarp.Patching;
[HarmonyPatch]
internal static class SettingsManagerPatcher
{
    internal static List<SettingsSubMenu> AllExtrasSettingsMenus = new();
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SettingsMenuManager))]
    [HarmonyPatch(nameof(SettingsMenuManager.ResetAllSettings))]
    internal static void ResetModsMenu()
    {
        foreach (var menu in AllExtrasSettingsMenus.Where(menu => menu != null))
        {
            menu.Revert();
        }
    }
}