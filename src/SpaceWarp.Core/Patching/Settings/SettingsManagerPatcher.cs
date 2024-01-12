using HarmonyLib;
using KSP.UI;

namespace SpaceWarp.Patching.Settings;
[HarmonyPatch]
internal static class SettingsManagerPatcher
{
    internal static readonly List<SettingsSubMenu> AllExtrasSettingsMenus = new();

    [HarmonyPatch(typeof(SettingsMenuManager), nameof(SettingsMenuManager.ResetAllSettings))]
    [HarmonyPostfix]
    internal static void ResetModsMenu()
    {
        foreach (var menu in AllExtrasSettingsMenus.Where(menu => menu != null))
        {
            menu.Revert();
        }
    }
}