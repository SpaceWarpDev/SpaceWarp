using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using ReduxLib.Configuration;
using ReduxLib.Configuration.Attributes;
using SpaceWarp2.Modules;
using SpaceWarp2.API.Loading;
using SpaceWarp2.UI.API.Appbar;
using SpaceWarp2.UI.AvcDialog;
using SpaceWarp2.UI.Backend.UI.Appbar;
using SpaceWarp2.UI.Console;
using SpaceWarp2.UI.ModList;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

// using SpaceWarp.API.UI.Appbar;

namespace SpaceWarp2.UI;

/// <summary>
/// The UI module for Space Warp.
/// </summary>
[PublicAPI]
public class UI : SpaceWarpModule
{
    /// <inheritdoc/>
    public override string Name => "SpaceWarp.UI";

    public static UI Instance;

    [ConfigSection("Debug Console", loc: "Menu/Settings/Sections/DebugConsole")]
    [ConfigValue("Color Error",
        "The color for log messages that have the level: Error/Fatal (bolded)",
        nameLoc: "Menu/Settings/ColorError",
        descLoc: "Menu/Settings/Description/ColorError")]
    internal Color ConfigErrorColor = Color.red;

    [ConfigValue("Color Warning",
        "The color for log messages that have the level: Warning",
        nameLoc: "Menu/Settings/ColorWarning",
        descLoc: "Menu/Settings/Description/ColorWarning")]
    internal Color ConfigWarningColor = Color.yellow;

    [ConfigValue("Color Message",
        "The color for log messages that have the level: Message",
        nameLoc: "Menu/Settings/ColorMessage",
        descLoc: "Menu/Settings/Description/ColorMessage")]
    internal Color ConfigMessageColor = Color.white;

    [ConfigValue("Color Info",
        "The color for log messages that have the level: Info",
        nameLoc: "Menu/Settings/ColorInfo",
        descLoc: "Menu/Settings/Description/ColorInfo")]
    internal Color ConfigInfoColor = Color.cyan;

    [ConfigValue("Color Debug",
        "The color for log messages that have the level: Debug",
        nameLoc: "Menu/Settings/ColorDebug",
        descLoc: "Menu/Settings/Description/ColorDebug")]
    internal Color ConfigDebugColor = Color.green;

    [ConfigValue("Color All",
        "The color for log messages that have the level: All",
        nameLoc: "Menu/Settings/ColorAll",
        descLoc: "Menu/Settings/Description/ColorAll")]
    internal Color ConfigAllColor = Color.magenta;

    [ConfigValue("Show Console Button",
        "Show console button in app.bar, requires restart",
        nameLoc: "Menu/Settings/ShowConsoleButton",
        descLoc: "Menu/Settings/Description/ShowConsoleButton")]
    internal bool ConfigShowConsoleButton;

    [ConfigValue("Show Timestamps",
        "Show time stamps in debug console",
        nameLoc: "Menu/Settings/ShowTimestamps",
        descLoc: "Menu/Settings/Description/ShowTimestamps")]
    internal bool ConfigShowTimeStamps = true;

    [ConfigValue("Timestamp Format",
        "The format for the timestamps in the debug console.",
        nameLoc: "Menu/Settings/TimestampFormat",
        descLoc: "Menu/Settings/Description/TimestampFormat")]
    internal string ConfigTimeStampFormat = "HH:mm:ss.fff";

    [ConfigValue("Message Limit",
        "The maximum number of messages to keep in the debug console.",
        nameLoc: "Menu/Settings/MessageLimit",
        descLoc: "Menu/Settings/Description/MessageLimit")]
    internal int ConfigDebugMessageLimit = 1000;

    [ConfigSection("Version Checking", loc: "Menu/Settings/Sections/VersionChecking")]
    [ConfigValue("Show Warning for Outdated Mods",
        "Whether or not Space Warp should display a warning in main menu if there are outdated mods",
        nameLoc: "Menu/Settings/ShowWarningForOutdatedMods",
        descLoc: "Menu/Settings/Description/ShowWarningForOutdatedMods")]
    internal bool ConfigShowMainMenuWarningForOutdatedMods = true;

    [ConfigValue("Show Warning for Errored Mods",
        "Whether or not Space Warp should display a warning in main menu if there are errored mods",
        nameLoc: "Menu/Settings/ShowWarningForErroredMods",
        descLoc: "Menu/Settings/Description/ShowWarningForErroredMods")]
    internal bool ConfigShowMainMenuWarningForErroredMods = true;
    internal ModListController ModListController;
    internal SpaceWarpConsole SpaceWarpConsole = null!;
    internal SpaceWarpConsoleLogListener SpaceWarpConsoleLogListener = null!;

    /// <inheritdoc/>
    public override void LoadModule()
    {
        IAppbarBackend.Instance.AppBarInFlightSubscriber.AddListener(Appbar.LoadAllButtons);
        IAppbarBackend.Instance.AppBarOABSubscriber.AddListener(Appbar.LoadOABButtons);
        IAppbarBackend.Instance.AppBarKSCSubscriber.AddListener(Appbar.LoadKSCButtons);
        Instance = this;
        ModuleConfiguration.Bind(this);
        SpaceWarpConsoleLogListener = new SpaceWarpConsoleLogListener(this);

        Loading.AddAddressablesLoadingAction<VisualTreeAsset>("Loading Space Warp UI Assets", "spacewarp-ui", true, OnSpaceWarpUILoad);

    }

    private static Dictionary<string, VisualTreeAsset> _uiAssets = new();

    public static VisualTreeAsset ListEntry => _uiAssets["modlistitem"];
    public static VisualTreeAsset DependencyEntry => _uiAssets["modlistdependency"];

    private static void OnSpaceWarpUILoad(VisualTreeAsset asset)
    {
        Instance.ModuleLogger.LogInfo($"Loading {asset.name}");
        _uiAssets[asset.name.ToLowerInvariant()] = asset;
    }

    /// <inheritdoc/>
    public override void PreInitializeModule()
    {
    }

    /// <inheritdoc/>
    public override void InitializeModule()
    {

        ModuleLogger.LogInfo("Initializing UI");
        if (VersionChecking.VersionChecking.Instance.ConfigFirstLaunch.Value)
        {
            var ui = ReduxLib.ReduxLib.GetAlwaysLoadedObject("Version Checking");
            ui.SetActive(true);

            // Generate a prompt for whether or not space warp should check mod versions
            var avcDialogUxml = _uiAssets["avcdialog"];

            var windowOptions = WindowOptions.Default;
            windowOptions.WindowId = "Space Warp AVC Dialog";
            windowOptions.Parent = ui.transform;
            var avcDialog = Window.Create(windowOptions, avcDialogUxml);

            var avcDialogController = avcDialog.gameObject.AddComponent<AvcDialogController>();
            avcDialogController.Module = VersionChecking.VersionChecking.Instance;
        }

        InitializeUI();
    }

    private static void Persist(Object obj)
    {
        Object.DontDestroyOnLoad(obj);
        obj.hideFlags |= HideFlags.HideAndDontSave;
    }

    /// <inheritdoc/>
    public override void PostInitializeModule()
    {
        ModuleLogger.LogInfo("Post Initializing UI");
        InitializeSpaceWarpDetailsFoldout();
        ModListController.AddMainMenuItem();
    }

    /// <inheritdoc/>
    public override List<string> Prerequisites => new() {"SpaceWarp.VersionChecking"};

    private void InitializeUI()
    {
        var ui = ReduxLib.ReduxLib.GetAlwaysLoadedObject("Space Warp UI");
        ui.SetActive(true);

        // var modListUxml = AssetManager.GetAsset<VisualTreeAsset>(
        //     $"{SpaceWarpPlugin.ModGuid}/modlist/ui/modlist/modlist.uxml"
        // );
        var modListUxml = _uiAssets["modlist"];

        var modListOptions = WindowOptions.Default;
        modListOptions.WindowId = "Space Warp Mod List";
        modListOptions.Parent = ui.transform;
        var modList = Window.Create(modListOptions, modListUxml);
        ModListController = modList.gameObject.AddComponent<ModListController>();

        var swConsoleUxml = _uiAssets["console"];
        
        var swConsoleOptions = WindowOptions.Default;
        swConsoleOptions.WindowId = "space-warp-console";
        swConsoleOptions.Parent = ui.transform;
        swConsoleOptions.MoveOptions = MoveOptions.Default with
        {
            HandleElementName = "title-bar"
        };
        swConsoleOptions.ResizeOptions = ResizeOptions.Default with
        {
            IsResizingEnabled = true,
            MinWidth = 760,
            MinHeight = 520
        };
        var swConsole = Window.Create(swConsoleOptions, swConsoleUxml);
        swConsole.Hide();
        SpaceWarpConsole = swConsole.gameObject.AddComponent<SpaceWarpConsole>();
    }

    private static VisualElement GenerateSpaceWarpModulesText()
    {
        var detailsContainer = new ScrollView();
        var websiteContainer = new VisualElement();
        websiteContainer.style.flexDirection = FlexDirection.Row;
        detailsContainer.Add(websiteContainer);
        var websiteHeader = new TextElement()
        {
            text = "Wiki: "
        };
        websiteContainer.Add(websiteHeader);
        var websiteLink = new Button()
        {
            text = "https://wiki.spacewarp.org"
        };
        websiteLink.AddToClassList("link");
        websiteLink.RegisterCallback<ClickEvent>(_ => Application.OpenURL(websiteLink.text));
        websiteContainer.Add(websiteLink);
        var loadedModules = new TextElement();
        detailsContainer.Add(loadedModules);
        loadedModules.visible = true;
        loadedModules.style.display = DisplayStyle.Flex;
        detailsContainer.visible = true;
        detailsContainer.style.display = DisplayStyle.Flex;
        var str = "Loaded modules: ";
        foreach (var module in ModuleManager.AllSpaceWarpModules)
        {
            str += $"\n- {module.Name}";
        }

        loadedModules.text = str;
        return detailsContainer;
    }
    private static void InitializeSpaceWarpDetailsFoldout()
    {
        API.ModList.RegisterDetailsFoldoutGenerator(SpaceWarpPlugin.Instance.SWMetadata.Guid, GenerateSpaceWarpModulesText);
        return;

    }
}
