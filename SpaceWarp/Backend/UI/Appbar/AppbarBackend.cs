// Attribution Notice To Lawrence/HatBat of https://github.com/Halbann/LazyOrbit
// This file is licensed under https://creativecommons.org/licenses/by-sa/4.0/

using System;
using System.Collections;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using I2.Loc;
using KSP;
using KSP.Api;
using KSP.Api.CoreTypes;
using KSP.Game;
using KSP.OAB;
using KSP.Sim.impl;
using KSP.UI.Binding;
using KSP.UI.Flight;
using SpaceWarp.API.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static SpaceWarp.Backend.UI.Appbar.AppbarBackend;
using Object = UnityEngine.Object;

namespace SpaceWarp.Backend.UI.Appbar;

internal static class AppbarBackend
{
    
    private static readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("ToolbarBackend");

    #region Flight App Bar

    private static UIValue_WriteBool_Toggle trayState;

    public static GameObject AddButton(string buttonText, Sprite buttonIcon, string buttonId, Action<bool> function)
    {
        // Find the resource manager button and "others" group.

        // Say the magic words...
        GameObject list = GameObject.Find("GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Popup Canvas/Container/ButtonBar/BTN-App-Tray/appbar-others-group");
        GameObject resourceManger = list?.GetChild("BTN-Resource-Manager");

        if (list == null || resourceManger == null)
        {
            _logger.LogInfo("Couldn't find appbar.");
            return null;
        }

        trayState = list.transform.parent.gameObject.GetComponent<UIValue_WriteBool_Toggle>();

        // Clone the resource manager button.
        GameObject appButton = Object.Instantiate(resourceManger, list.transform);
        appButton.name = buttonId;

        // Change the text.
        TextMeshProUGUI text = appButton.GetChild("Content").GetChild("TXT-title").GetComponent<TextMeshProUGUI>();
        text.text = buttonText;

        Localize localizer = text.gameObject.GetComponent<Localize>();
        if (localizer)
        {
            Object.Destroy(localizer);
        }

        // Change the icon.
        GameObject icon = appButton.GetChild("Content").GetChild("GRP-icon");
        Image image = icon.GetChild("ICO-asset").GetComponent<Image>();
        image.sprite = buttonIcon;

        // Add our function call to the toggle.
        ToggleExtended utoggle = appButton.GetComponent<ToggleExtended>();
        utoggle.onValueChanged.AddListener(state => function(state));
        utoggle.onValueChanged.AddListener(state => SetTrayState(false));

        // Set the initial state of the button.
        UIValue_WriteBool_Toggle toggle = appButton.GetComponent<UIValue_WriteBool_Toggle>();
        toggle.BindValue(new Property<bool>(false));

        _logger.LogInfo($"Added appbar button: {buttonId}");

        return appButton;
    }

    public static void SetTrayState(bool state)
    {
        if (trayState == null)
            return;

        trayState.SetValue(state);
    }

    #endregion

    #region OAB App Bar

    private static GameObject _oabTray;
    private static GameObject OABTray
    {
        get
        {
            if (_oabTray == null)
                return _oabTray = CreateOABTray();

            return _oabTray;
        }
        set => _oabTray = value;
    }

    private static Property<bool> _oabState;

    private static GameObject CreateOABTray()
    {
        _logger.LogInfo("Creating OAB app button tray...");

        // Find the OAB app bar and the kerbal manager button.

        // Say the magic words...
        GameObject oabAppBar = GameObject.Find("OAB(Clone)/HUDSpawner/HUD/widget_SideBar/widget_sidebarNav");
        GameObject kerbalManager = oabAppBar?.GetChild("button_kerbal-manager");

        if (oabAppBar == null || kerbalManager == null)
        {
            _logger.LogError("Couldn't find OAB appbar.");
            return null;
        }

        // Clone the kerbal manager button.
        GameObject oabTrayButton = Object.Instantiate(kerbalManager, oabAppBar.transform);
        oabTrayButton.name = "OAB-AppTrayButton";

        // Set the initial state of the button.
        UIValue_WriteBool_Toggle toggle = oabTrayButton.GetComponent<UIValue_WriteBool_Toggle>();
        Property<bool> state = new Property<bool>(false);
        toggle.BindValue(state);
        _oabState = state;

        // Set the button icon.
        Image image = oabTrayButton.GetComponent<Image>();
        Texture2D tex = AssetManager.GetAsset<Texture2D>("spacewarp/images/oabTrayButton.png");
        tex.filterMode = FilterMode.Point;
        image.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));

        // Delete the old tooltip.
        var tooltip = oabTrayButton.GetComponent<ObjectAssemblyBuilderTooltipDisplay>();
        Object.DestroyImmediate(tooltip);

        // Create a new tooltip holder, having it on a separate game object prevents
        // the tooltip from showing up on children of the tray button.
        GameObject tooltipObject = new GameObject("OAB-AppTrayTooltip");

        tooltipObject.AddComponent<RectTransform>().CopyFrom(oabTrayButton.GetComponent<RectTransform>());
        tooltipObject.transform.SetParent(oabTrayButton.transform);
        tooltipObject.transform.localPosition = Vector3.zero;

        // ObjectAssemblyBuilderTooltipDisplay seems to require an image to work.
        Image tooltipImage = tooltipObject.AddComponent<Image>();
        tooltipImage.color = new Color(0, 0, 0, 0);

        // Create the tooltip itself.
        tooltip = tooltipObject.AddComponent<ObjectAssemblyBuilderTooltipDisplay>();
        tooltip.GetType().GetField("tooltipText", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(tooltip, "App Tray");

        // Clone the tray from the flight UI.
        GameObject trayButton = GameManager.Instance.Game.UI.GetPopupCanvas().gameObject.GetChild("BTN-App-Tray");
        GameObject oabTray = Object.Instantiate(trayButton.GetChild("appbar-others-group"), oabTrayButton.transform);
        oabTray.name = "OAB-AppTray";

        // Bind the tray state with our button.
        oabTray.GetComponent<UIValue_ReadBool_SetAlpha>().BindValue(state);

        // Change the background colour.
        var oabAppBarBG = oabAppBar.GetChild("BG-AppBar").GetComponent<Image>();
        oabTray.GetComponent<Image>().color = oabAppBarBG.color;

        // Delete the existing buttons in the tray.
        for (int i = 0; i < oabTray.transform.childCount; i++)
        {
            var child = oabTray.transform.GetChild(i);

            if (child.name.Contains("ELE-border"))
                continue;

            Object.Destroy(child.gameObject);
        }

        _logger.LogInfo("Created OAB app button tray.");

        return oabTray;
    }

    public static GameObject AddOABButton(string buttonText, Sprite buttonIcon, string buttonId, Action<bool> function)
    {
        _logger.LogInfo($"Adding OAB app bar button: {buttonId}.");

        // Find the resource manager button.
        GameObject list = GameObject.Find("GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Popup Canvas/Container/ButtonBar/BTN-App-Tray/appbar-others-group");
        GameObject resourceManger = list?.GetChild("BTN-Resource-Manager");

        if (resourceManger == null)
        {
            _logger.LogError("Couldn't find the appbar.");
            return null;
        }

        // Clone the resource manager button.
        GameObject appButton = Object.Instantiate(resourceManger, OABTray.transform);
        appButton.name = buttonId;

        // Change the text.
        TextMeshProUGUI text = appButton.GetChild("Content").GetChild("TXT-title").GetComponent<TextMeshProUGUI>();
        text.text = buttonText;

        Localize localizer = text.gameObject.GetComponent<Localize>();
        if (localizer)
            Object.Destroy(localizer);

        // Change the icon.
        GameObject icon = appButton.GetChild("Content").GetChild("GRP-icon");
        Image image = icon.GetChild("ICO-asset").GetComponent<Image>();
        image.sprite = buttonIcon;

        // Add our function call to the toggle.
        ToggleExtended utoggle = appButton.GetComponent<ToggleExtended>();
        utoggle.onValueChanged.AddListener(state => function(state));

        // Set the initial state of the button.
        UIValue_WriteBool_Toggle toggle = appButton.GetComponent<UIValue_WriteBool_Toggle>();
        toggle.BindValue(new Property<bool>(false));

        // Bind the action to close the tray after pressing the button.
        Action action = () => SetOABTrayState(false);
        appButton.GetComponent<UIAction_Void_Toggle>().BindAction(new DelegateAction(action));

        _logger.LogInfo($"Added appbar button: {buttonId}");

        return appButton;
    }

    public static void SetOABTrayState(bool state)
    {
        if (_oabTray == null)
            return;

        _oabState.SetValue(state);
    }

    #endregion

    public static readonly UnityEvent AppBarOABSubscriber = new();
    public static readonly UnityEvent AppBarInFlightSubscriber = new();

    internal static void SubscriberSchedulePing(AppbarEvent type)
    {
        _logger.LogInfo($"App bar creation event started: {type}");

        GameObject gameObject = new GameObject();
        ToolbarBackendObject waiterObject = gameObject.AddComponent<ToolbarBackendObject>();

        switch (type)
        {
            case AppbarEvent.Flight:
                waiterObject.creationEvent = AppBarInFlightSubscriber;
                break;
            case AppbarEvent.OAB:
                waiterObject.creationEvent = AppBarOABSubscriber;
                break;
            default:
                break;
        }

        waiterObject.type = type;
        gameObject.SetActive(true);
    }

    internal enum AppbarEvent
    {
        Flight,
        OAB
    }
}

internal class ToolbarBackendObject : KerbalBehavior
{
    internal UnityEvent creationEvent;
    internal AppbarEvent type;

    public new void Start()
    {
        StartCoroutine(awaiter());
    }

    private IEnumerator awaiter()
    {
        switch (type)
        {
            case AppbarEvent.Flight:
                yield return new WaitForSeconds(1);
                break;
            case AppbarEvent.OAB:
                yield return new WaitForFixedUpdate();
                break;
            default:
                yield return new WaitForSeconds(1);
                break;
        }

        creationEvent.Invoke();
        Destroy(this);
    }
}

//TODO: Much better way of doing this
[HarmonyPatch(typeof(UIFlightHud))]
[HarmonyPatch("Start")]
internal class ToolbarBackendAppBarPatcher
{
    public static void Postfix(UIFlightHud __instance) =>
        AppbarBackend.SubscriberSchedulePing(AppbarBackend.AppbarEvent.Flight);
}

[HarmonyPatch(typeof(OABSideBar))]
[HarmonyPatch("Start")]
internal class ToolbarBackendOABSideBarPatcher
{
    public static void Postfix(OABSideBar __instance) =>
        AppbarBackend.SubscriberSchedulePing(AppbarBackend.AppbarEvent.OAB);
}
