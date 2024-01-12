using HarmonyLib;
using I2.Loc;
using JetBrains.Annotations;
using KSP.Api.CoreTypes;
using KSP.Game.StartupFlow;
using TMPro;
using UnityObject = UnityEngine.Object;

namespace SpaceWarp.Patching.MainMenu;

[HarmonyPatch(typeof(LandingHUD), nameof(LandingHUD.Start))]
internal class MainMenuPatcher
{
    internal static event Action MainMenuLoaded;

    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public static void Postfix(LandingHUD __instance)
    {
        var menuItemsGroupTransform = __instance.transform.FindChildEx("MenuItemsGroup");
        var singleplayerButtonTransform = menuItemsGroupTransform.FindChildEx("Singleplayer");

        foreach (var menuButtonToBeAdded in API.UI.MainMenu.MenuButtonsToBeAdded)
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

            LocalizationManager.OnLocalizeEvent += () => tmp.SetText(menuButtonToBeAdded.name);
        }

        foreach (var localizedMenuButtonToBeAdded in API.UI.MainMenu.LocalizedMenuButtonsToBeAdded)
        {
            var newButton = UnityObject.Instantiate(
                singleplayerButtonTransform.gameObject,
                menuItemsGroupTransform,
                false
            );
            newButton.name = localizedMenuButtonToBeAdded.term;

            // Move the button to be above the Exit button.
            newButton.transform.SetSiblingIndex(newButton.transform.GetSiblingIndex() - 1);

            // Rebind the button's action to call the action
            var uiAction = newButton.GetComponent<UIAction_Void_Button>();
            DelegateAction action = new();
            action.BindDelegate(() => localizedMenuButtonToBeAdded.onClicked.Invoke());
            uiAction.BindAction(action);
            var localize = newButton.GetComponentInChildren<Localize>();
            localize.SetTerm(localizedMenuButtonToBeAdded.term);
        }

        MainMenuLoaded?.Invoke();
    }
}