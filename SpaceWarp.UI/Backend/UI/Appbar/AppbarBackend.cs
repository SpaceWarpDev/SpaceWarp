// Attribution Notice To Lawrence/HatBat of https://github.com/Halbann/LazyOrbit
// This file is licensed under https://creativecommons.org/licenses/by-sa/4.0/

using System;
using System.Collections;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using I2.Loc;
using KSP;
using KSP.Api.CoreTypes;
using KSP.Game;
using KSP.OAB;
using KSP.Sim.impl;
using KSP.UI;
using KSP.UI.Binding;
using KSP.UI.Flight;
using SpaceWarp.API.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static SpaceWarp.Backend.UI.Appbar.AppbarBackend;
using UnityObject = UnityEngine.Object;

namespace SpaceWarp.Backend.UI.Appbar;

internal static class AppbarBackend
{
    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("ToolbarBackend");

    public static readonly UnityEvent AppBarOABSubscriber = new();
    public static readonly UnityEvent AppBarInFlightSubscriber = new();
    public static readonly UnityEvent AppBarKSCSubscriber = new();

    internal static void SubscriberSchedulePing(AppbarEvent type)
    {
        Logger.LogInfo($"App bar creation event started: {type}");

        var gameObject = new GameObject();
        var waiterObject = gameObject.AddComponent<ToolbarBackendObject>();

        waiterObject.CreationEvent = type switch
        {
            AppbarEvent.Flight => AppBarInFlightSubscriber,
            AppbarEvent.OAB => AppBarOABSubscriber,
            AppbarEvent.KSC => AppBarKSCSubscriber,
            _ => waiterObject.CreationEvent
        };

        waiterObject.Type = type;
        gameObject.SetActive(true);
    }

    internal enum AppbarEvent
    {
        Flight,
        OAB,
        KSC
    }

    #region Flight App Bar

    private static UIValue_WriteBool_Toggle _trayState;

    public static GameObject AddButton(string buttonText, Sprite buttonIcon, string buttonId, Action<bool> function)
    {
        // Find the resource manager button and "others" group.

        // Say the magic words...
        var list = GameObject.Find(
            "GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Scaled Popup Canvas/Container/ButtonBar/BTN-App-Tray/appbar-others-group");
        var resourceManger = list != null ? list.GetChild("BTN-Resource-Manager") : null;

        if (list == null || resourceManger == null)
        {
            Logger.LogInfo("Couldn't find appbar.");
            return null;
        }

        _trayState = list.transform.parent.gameObject.GetComponent<UIValue_WriteBool_Toggle>();

        // Clone the resource manager button.
        var appButton = UnityObject.Instantiate(resourceManger, list.transform);
        appButton.name = buttonId;

        // Change the text.
        var text = appButton.GetChild("Content").GetChild("TXT-title").GetComponent<TextMeshProUGUI>();
        text.text = buttonText;

        var localizer = text.gameObject.GetComponent<Localize>();
        if (localizer)
        {
            UnityObject.Destroy(localizer);
        }

        // Change the icon.
        var icon = appButton.GetChild("Content").GetChild("GRP-icon");
        var image = icon.GetChild("ICO-asset").GetComponent<Image>();
        image.sprite = buttonIcon;

        // Add our function call to the toggle.
        var utoggle = appButton.GetComponent<ToggleExtended>();
        utoggle.onValueChanged.AddListener(state => function(state));
        utoggle.onValueChanged.AddListener(_ => SetTrayState(false));

        // Set the initial state of the button.
        var toggle = appButton.GetComponent<UIValue_WriteBool_Toggle>();
        toggle.BindValue(new Property<bool>(false));

        Logger.LogInfo($"Added appbar button: {buttonId}");

        return appButton;
    }

    public static void SetTrayState(bool state)
    {
        if (_trayState == null)
        {
            return;
        }

        _trayState.SetValue(state);
    }

    #endregion

    #region OAB App Bar

    private static GameObject _oabTray;

    private static GameObject OABTray
    {
        get
        {
            if (_oabTray == null)
            {
                return _oabTray = CreateOABTray();
            }

            return _oabTray;
        }
    }

    private static Property<bool> _oabState;

    private static GameObject CreateOABTray()
    {
        Logger.LogInfo("Creating OAB app button tray...");

        // Find the OAB app bar and the kerbal manager button.

        // Say the magic words...
        var oabAppBar = GameObject.Find("OAB(Clone)/HUDSpawner/HUD/widget_SideBar/widget_sidebarNav");
        var kerbalManager = oabAppBar != null ? oabAppBar.GetChild("button_kerbal-manager") : null;

        if (oabAppBar == null || kerbalManager == null)
        {
            Logger.LogError("Couldn't find OAB appbar.");
            return null;
        }

        // Clone the kerbal manager button.
        var oabTrayButton = UnityObject.Instantiate(kerbalManager, oabAppBar.transform);
        oabTrayButton.name = "OAB-AppTrayButton";

        // Set the initial state of the button.
        var toggle = oabTrayButton.GetComponent<UIValue_WriteBool_Toggle>();
        var state = new Property<bool>(false);
        toggle.BindValue(state);
        _oabState = state;

        // Set the button icon.
        var image = oabTrayButton.GetComponent<Image>();
        var tex = AssetManager.GetAsset<Texture2D>($"{SpaceWarpPlugin.ModGuid}/images/oabTrayButton.png");
        tex.filterMode = FilterMode.Point;
        image.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));

        // Delete the old tooltip.
        var tooltip = oabTrayButton.GetComponent<ObjectAssemblyBuilderTooltipDisplay>();
        UnityObject.DestroyImmediate(tooltip);

        // Create a new tooltip holder, having it on a separate game object prevents
        // the tooltip from showing up on children of the tray button.
        var tooltipObject = new GameObject("OAB-AppTrayTooltip");

        tooltipObject.AddComponent<RectTransform>().CopyFrom(oabTrayButton.GetComponent<RectTransform>());
        tooltipObject.transform.SetParent(oabTrayButton.transform);
        tooltipObject.transform.localPosition = Vector3.zero;

        // ObjectAssemblyBuilderTooltipDisplay seems to require an image to work.
        var tooltipImage = tooltipObject.AddComponent<Image>();
        tooltipImage.color = new Color(0, 0, 0, 0);

        // Create the tooltip itself.
        tooltip = tooltipObject.AddComponent<ObjectAssemblyBuilderTooltipDisplay>();
        tooltip.GetType().GetField("tooltipText", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(tooltip, "App Tray");

        // Clone the tray from the flight UI.
        var trayButton = GameManager.Instance.Game.UI.GetScaledPopupCanvas().gameObject.GetChild("BTN-App-Tray");
        var oabTray = UnityObject.Instantiate(trayButton.GetChild("appbar-others-group"), oabTrayButton.transform);
        oabTray.name = "OAB-AppTray";

        // Bind the tray state with our button.
        oabTray.GetComponent<UIValue_ReadBool_SetAlpha>().BindValue(state);

        // Change the background colour.
        var oabAppBarBg = oabAppBar.GetChild("BG-AppBar").GetComponent<Image>();
        oabTray.GetComponent<Image>().color = oabAppBarBg.color;

        // Delete the existing buttons in the tray.
        for (var i = 0; i < oabTray.transform.childCount; i++)
        {
            var child = oabTray.transform.GetChild(i);

            if (child.name.Contains("ELE-border"))
            {
                continue;
            }

            UnityObject.Destroy(child.gameObject);
        }

        Logger.LogInfo("Created OAB app button tray.");

        return oabTray;
    }

    public static void AddOABButton(string buttonText, Sprite buttonIcon, string buttonId, Action<bool> function)
    {
        Logger.LogInfo($"Adding OAB app bar button: {buttonId}.");

        // Find the resource manager button.
        var list = GameObject.Find(
            "GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Scaled Popup Canvas/Container/ButtonBar/BTN-App-Tray/appbar-others-group");
        var resourceManger = list != null ? list.GetChild("BTN-Resource-Manager") : null;

        if (resourceManger == null)
        {
            Logger.LogError("Couldn't find the appbar.");
            return;
        }

        // Clone the resource manager button.
        var appButton = UnityObject.Instantiate(resourceManger, OABTray.transform);
        appButton.name = buttonId;

        // Change the text.
        var text = appButton.GetChild("Content").GetChild("TXT-title").GetComponent<TextMeshProUGUI>();
        text.text = buttonText;

        var localizer = text.gameObject.GetComponent<Localize>();
        if (localizer)
        {
            UnityObject.Destroy(localizer);
        }

        // Change the icon.
        var icon = appButton.GetChild("Content").GetChild("GRP-icon");
        var image = icon.GetChild("ICO-asset").GetComponent<Image>();
        image.sprite = buttonIcon;

        // Add our function call to the toggle.
        var utoggle = appButton.GetComponent<ToggleExtended>();
        utoggle.onValueChanged.AddListener(state =>
        {
            Logger.LogInfo($"{buttonId}({state})");
            function(state);
        });

        // Set the initial state of the button.
        var toggle = appButton.GetComponent<UIValue_WriteBool_Toggle>();
        toggle.BindValue(new Property<bool>(false));

        // Bind the action to close the tray after pressing the button.
        void Action() => SetOABTrayState(false);
        appButton.GetComponent<UIAction_Void_Toggle>().BindAction(new DelegateAction((Action)Action));

        Logger.LogInfo($"Added OAB appbar button: {buttonId}");
    }

    private static void SetOABTrayState(bool state)
    {
        if (_oabTray == null)
        {
            return;
        }

        _oabState.SetValue(state);
    }

    #endregion

    #region KSC App Bar

    private static GameObject _kscTray;

    private static GameObject KSCTray
    {
        get
        {
            if (_kscTray == null)
            {
                return _kscTray = CreateKSCTray();
            }

            return _kscTray;
        }
    }

    private static GameObject CreateKSCTray()
    {
        Logger.LogInfo("Creating KSC app tray...");

        // Find the KSC launch locations menu item; it will be used for cloning the app tray

        // Get the Launch Pads menu item
        var kscMenu = GameObject.Find("GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Main Canvas/KSCMenu(Clone)/LandingPanel/InteriorWindow/MenuButtons/Content/Menu");
        var launchLocationsButton = kscMenu != null ? kscMenu.GetChild("LaunchLocationFlyoutHeaderToggle") : null;

        if (kscMenu == null || launchLocationsButton == null)
        {
            Logger.LogError("Couldn't find Launch Pads menu item for cloning the KSC app tray.");
            return null;
        }

        // Clone it, add it to the menu and rename it
        var kscAppTrayButton = UnityObject.Instantiate(launchLocationsButton, kscMenu.transform);
        kscAppTrayButton.name = "KSC-AppTrayButton";

        // Set the button icon (use OAB app tray icon)
        var image = kscAppTrayButton.GetChild("Header").GetChild("Content").GetChild("Icon Panel").GetChild("icon").GetComponent<Image>();
        var tex = AssetManager.GetAsset<Texture2D>($"{SpaceWarpPlugin.ModGuid}/images/oabTrayButton.png");
        tex.filterMode = FilterMode.Point;
        image.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));

        // Change the text to APPS
        var title = kscAppTrayButton.GetChild("Header").GetChild("Content").GetChild("Title");
        {
            // Suppress renaming of the button to Launchpad
            var localizer = title.GetComponent<Localize>();
            if (localizer)
            {
                UnityObject.Destroy(localizer);
            }
            var text = title.GetComponent<TextMeshProUGUI>();
            text.text = "Apps";
        }

        // Get the popup tray and rename it
        var kscAppTray = kscAppTrayButton.GetChild("LaunchLocationsFlyoutTarget");
        kscAppTray.name = "KSC-AppTray";

        // Delete existing buttons and separators in the tray
        for (var i = 0; i < kscAppTray.transform.childCount; i++)
        {
            var child = kscAppTray.transform.GetChild(i);

            // Destroy all objects inside the tray, but keep the arrow ("thingy") that points to the menu button
            if (!child.name.ToLowerInvariant().Contains("thingy"))
                UnityObject.Destroy(child.gameObject);
        }

        Logger.LogInfo("Created KSC app tray.");

        return kscAppTray;
    }

    public static void AddKSCButton(string buttonText, Sprite buttonIcon, string buttonId, Action function)
    {
        Logger.LogInfo($"Adding KSC appbar button: {buttonId}.");

        // Grab the Launchpad_1 button, clone it and convert it to a mod launching button

        // Find the Launchpad_1 button.
        var kscLaunchLocationsFlyoutTarget = GameObject.Find(
            "GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Main Canvas/KSCMenu(Clone)/LandingPanel/InteriorWindow/MenuButtons/Content/Menu/LaunchLocationFlyoutHeaderToggle/LaunchLocationsFlyoutTarget");
        var launchPadButton = kscLaunchLocationsFlyoutTarget != null ? kscLaunchLocationsFlyoutTarget.GetChild("Launchpad_1") : null;

        if (launchPadButton == null)
        {
            Logger.LogError("Couldn't find the KSC Launchpad_1 button.");
            return;
        }

        // Clone the button, add it to the popup tray and rename it
        var modButton = UnityObject.Instantiate(launchPadButton, KSCTray.transform);
        modButton.name = buttonId;

        // Change the text
        var modText = modButton.GetChild("Content").GetChild("Text (TMP)").GetComponent<TextMeshProUGUI>();
        modText.text = buttonText;

        // Suppress renaming of the button
        var localizer = modText.gameObject.GetComponent<Localize>();
        if (localizer)
        {
            UnityObject.Destroy(localizer);
        }

        // Change the icon
        var icon = modButton.GetChild("Icon");
        var image = icon.GetComponent<Image>();
        image.sprite = buttonIcon;

        // Remove previous onclick listeners and add the function that mod will use
        var buttonExtended = modButton.GetComponent<ButtonExtended>();
        var previousListeners = modButton.GetComponent<UIAction_String_ButtonExtended>();
        if (previousListeners)
        {
            UnityObject.Destroy(previousListeners);
        }
        buttonExtended.onClick.AddListener(() =>
        {
            Logger.LogInfo($"Mod button {buttonId} clicked.");
            function();

            // Hide the popup tray after the button is clicked
            var toggle = KSCTray.GetComponentInParent<ToggleExtended>();
            toggle.isOn = false;
        });

        Logger.LogInfo($"Added KSC appbar button: {buttonId}.");
    }

    #endregion
}

internal class ToolbarBackendObject : KerbalBehavior
{
    internal UnityEvent CreationEvent;
    internal AppbarEvent Type;

    public new void Start()
    {
        StartCoroutine(Awaiter());
    }

    private IEnumerator Awaiter()
    {
        yield return Type switch
        {
            AppbarEvent.Flight => new WaitForSeconds(1),
            AppbarEvent.OAB => new WaitForFixedUpdate(),
            AppbarEvent.KSC => new WaitForFixedUpdate(),
            _ => new WaitForSeconds(1)
        };

        CreationEvent.Invoke();
        Destroy(this);
    }
}

//TODO: Much better way of doing this
[HarmonyPatch(typeof(UIFlightHud))]
[HarmonyPatch("Start")]
internal class ToolbarBackendAppBarPatcher
{
    public static void Postfix()
    {
        SubscriberSchedulePing(AppbarEvent.Flight);
    }
}

[HarmonyPatch(typeof(OABSideBar))]
[HarmonyPatch("Start")]
internal class ToolbarBackendOABSideBarPatcher
{
    public static void Postfix()
    {
        SubscriberSchedulePing(AppbarEvent.OAB);
    }
}

[HarmonyPatch(typeof(KSCMenuManager))]
[HarmonyPatch("Start")]
internal class ToolbarBackendKSCPatcher
{
    public static void Postfix()
    {
        SubscriberSchedulePing(AppbarEvent.KSC);
    }
}
