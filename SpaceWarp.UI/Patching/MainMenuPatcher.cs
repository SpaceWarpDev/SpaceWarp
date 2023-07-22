using System;
using HarmonyLib;
using I2.Loc;
using KSP.Api.CoreTypes;
using KSP.Game.StartupFlow;
using SpaceWarp.API.UI;
using TMPro;

namespace SpaceWarp.Patching;

[HarmonyPatch(typeof(LandingHUD))]
[HarmonyPatch("Start")]
internal class MainMenuPatcher
{
    internal static event Action MainMenuLoaded;
    public static void Postfix(LandingHUD __instance)
    {
        var menuItemsGroupTransform = __instance.transform.FindChildEx("MenuItemsGroup");
        var singleplayerButtonTransform = menuItemsGroupTransform.FindChildEx("Singleplayer");

        foreach (var menuButtonToBeAdded in MainMenu.MenuButtonsToBeAdded)
        {
            var newButton =
                UnityObject.Instantiate(singleplayerButtonTransform.gameObject, menuItemsGroupTransform, false);
            newButton.name = menuButtonToBeAdded.name;

            // Move the button to be above the Exit button.
            newButton.transform.SetSiblingIndex(newButton.transform.GetSiblingIndex() - 1);

            // Rebind the button's action to call the action
            var uiAction = newButton.GetComponent<UIAction_Void_Button>();
            DelegateAction action = new();
            action.BindDelegate(() => menuButtonToBeAdded.onClicked.Invoke());
            uiAction.BindAction(action);

            // Set the label to "Mods".
            var tmp = newButton.GetComponentInChildren<TextMeshProUGUI>();

            tmp.SetText(menuButtonToBeAdded.name);
        }

        foreach (var localizedMenuButtonToBeAddded in MainMenu.LocalizedMenuButtonsToBeAdded)
        {var newButton =
                UnityObject.Instantiate(singleplayerButtonTransform.gameObject, menuItemsGroupTransform, false);
            newButton.name = localizedMenuButtonToBeAddded.term;

            // Move the button to be above the Exit button.
            newButton.transform.SetSiblingIndex(newButton.transform.GetSiblingIndex() - 1);

            // Rebind the button's action to call the action
            var uiAction = newButton.GetComponent<UIAction_Void_Button>();
            DelegateAction action = new();
            action.BindDelegate(() => localizedMenuButtonToBeAddded.onClicked.Invoke());
            uiAction.BindAction(action);
            var localize = newButton.GetComponentInChildren<Localize>();
            localize.SetTerm(localizedMenuButtonToBeAddded.term);
        }
        MainMenuLoaded?.Invoke();
    }
}