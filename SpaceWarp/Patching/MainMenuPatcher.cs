using HarmonyLib;
using KSP.Api.CoreTypes;
using SpaceWarp.API;
using SpaceWarp.API.Managers;
using TMPro;
using UnityEngine;

namespace SpaceWarp.Patching;

[HarmonyPatch(typeof(KSP.Game.StartupFlow.LandingHUD))]
[HarmonyPatch("Start")]
class MainMenuPatcher
{
    public static void Postfix(KSP.Game.StartupFlow.LandingHUD __instance)
    {

        Transform menuItemsGroupTransform = __instance.transform.FindChildEx("MenuItemsGroup");

        Transform singleplayerButtonTransform = menuItemsGroupTransform.FindChildEx("Singleplayer");

        GameObject modsButton = Object.Instantiate(singleplayerButtonTransform.gameObject, menuItemsGroupTransform, false);
        modsButton.name = "Mods";

        // Move the button to be above the Exit button.
        modsButton.transform.SetSiblingIndex(modsButton.transform.GetSiblingIndex() - 1);

        // Rebind the button's action to open the mod manager dialog.
        UIAction_Void_Button uiAction = modsButton.GetComponent<UIAction_Void_Button>();
        DelegateAction action = new DelegateAction();
        action.BindDelegate(ModsOnClick);
        uiAction.BindAction(action);

        // Set the label to "Mods".
        TextMeshProUGUI tmp = modsButton.GetComponentInChildren<TextMeshProUGUI>();

        tmp.SetText("Mods");

    }

    static void ModsOnClick()
    {
        bool found = ManagerLocator.TryGet(out SpaceWarpManager manager);

        if (found)
        {
            manager.ModListUI.ToggleVisible();
        }
    }
}