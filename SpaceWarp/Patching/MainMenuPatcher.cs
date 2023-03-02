using HarmonyLib;
using KSP.Api.CoreTypes;
using SpaceWarp.API.UI;
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

        foreach (var menuButtonToBeAdded in MainMenu.MenuButtonsToBeAdded)
        {
            GameObject newButton =
                Object.Instantiate(singleplayerButtonTransform.gameObject, menuItemsGroupTransform, false);
            newButton.name = menuButtonToBeAdded.name;

            // Move the button to be above the Exit button.
            newButton.transform.SetSiblingIndex(newButton.transform.GetSiblingIndex() - 1);

            // Rebind the button's action to call the action
            UIAction_Void_Button uiAction = newButton.GetComponent<UIAction_Void_Button>();
            DelegateAction action = new();
            action.BindDelegate(() => menuButtonToBeAdded.onClicked.Invoke());
            uiAction.BindAction(action);

            // Set the label to "Mods".
            TextMeshProUGUI tmp = newButton.GetComponentInChildren<TextMeshProUGUI>();

            tmp.SetText(menuButtonToBeAdded.name);
        }
    }
}
