using BepInEx.Bootstrap;
using JetBrains.Annotations;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.UI.Appbar;
using SpaceWarp.Backend.UI.Appbar;
using SpaceWarp.InternalUtilities;
using SpaceWarp.UI.AvcDialog;
using SpaceWarp.UI.Console;
using SpaceWarp.UI.ModList;
using SpaceWarp.UI.Settings;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using ConfigManager = ConfigurationManager.ConfigurationManager;

namespace SpaceWarp.Modules;

/// <summary>
/// The UI module for Space Warp.
/// </summary>
[PublicAPI]
public class UI : SpaceWarpModule
{
    /// <inheritdoc/>
    public override string Name => "SpaceWarp.UI";

    internal static UI Instance;

    internal ConfigValue<Color> ConfigAllColor;
    internal ConfigValue<bool> ConfigCheckVersions;
    internal ConfigValue<bool> ConfigShowMainMenuWarningForOutdatedMods;
    internal ConfigValue<bool> ConfigShowMainMenuWarningForErroredMods;
    internal ConfigValue<Color> ConfigDebugColor;
    internal ConfigValue<int> ConfigDebugMessageLimit;
    internal ConfigValue<Color> ConfigErrorColor;
    internal ConfigValue<Color> ConfigInfoColor;
    internal ConfigValue<Color> ConfigMessageColor;
    internal ConfigValue<bool> ConfigShowConsoleButton;
    internal ConfigValue<bool> ConfigShowTimeStamps;
    internal ConfigValue<string> ConfigTimeStampFormat;
    internal ConfigValue<Color> ConfigWarningColor;
    internal ConfigManager ConfigurationManager;
    internal ModListController ModListController;
    internal SpaceWarpConsole SpaceWarpConsole;

    /// <inheritdoc/>
    public override void LoadModule()
    {
        AppbarBackend.AppBarInFlightSubscriber.AddListener(Appbar.LoadAllButtons);
        AppbarBackend.AppBarOABSubscriber.AddListener(Appbar.LoadOABButtons);
        AppbarBackend.AppBarKSCSubscriber.AddListener(Appbar.LoadKSCButtons);
        Instance = this;
        ConfigErrorColor = new(ModuleConfiguration.Bind("Debug Console", "Color Error", Color.red,
            "The color for log messages that have the level: Error/Fatal (bolded)"));
        ConfigWarningColor = new(ModuleConfiguration.Bind("Debug Console", "Color Warning", Color.yellow,
            "The color for log messages that have the level: Warning"));
        ConfigMessageColor = new(ModuleConfiguration.Bind("Debug Console", "Color Message", Color.white,
            "The color for log messages that have the level: Message"));
        ConfigInfoColor = new(ModuleConfiguration.Bind("Debug Console", "Color Info", Color.cyan,
            "The color for log messages that have the level: Info"));
        ConfigDebugColor = new(ModuleConfiguration.Bind("Debug Console", "Color Debug", Color.green,
            "The color for log messages that have the level: Debug"));
        ConfigAllColor = new(ModuleConfiguration.Bind("Debug Console", "Color All", Color.magenta,
            "The color for log messages that have the level: All"));
        ConfigShowConsoleButton = new(ModuleConfiguration.Bind("Debug Console", "Show Console Button", false,
            "Show console button in app.bar, requires restart"));
        ConfigShowTimeStamps = new(ModuleConfiguration.Bind("Debug Console", "Show Timestamps", true,
            "Show time stamps in debug console"));
        ConfigTimeStampFormat = new(ModuleConfiguration.Bind("Debug Console", "Timestamp Format", "HH:mm:ss.fff",
            "The format for the timestamps in the debug console."));
        ConfigDebugMessageLimit = new(ModuleConfiguration.Bind("Debug Console", "Message Limit", 1000,
            "The maximum number of messages to keep in the debug console."));
        ConfigShowMainMenuWarningForOutdatedMods = new(ModuleConfiguration.Bind("Version Checking",
            "Show Warning for Outdated Mods", true,
            "Whether or not Space Warp should display a warning in main menu if there are outdated mods"));
        ConfigShowMainMenuWarningForErroredMods = new(ModuleConfiguration.Bind("Version Checking",
            "Show Warning for Errored Mods", true,
            "Whether or not Space Warp should display a warning in main menu if there are errored mods"));
        BepInEx.Logging.Logger.Listeners.Add(new SpaceWarpConsoleLogListener(this));
    }

    /// <inheritdoc/>
    public override void PreInitializeModule()
    {
    }

    /// <inheritdoc/>
    public override void InitializeModule()
    {
        ModuleLogger.LogInfo("Initializing UI");
        if (VersionChecking.Instance.ConfigFirstLaunch.Value)
        {
            var ui = new GameObject("Version Check Dialog");
            ui.Persist();
            ui.SetActive(true);

            // Generate a prompt for whether or not space warp should check mod versions
            var avcDialogUxml =
                AssetManager.GetAsset<VisualTreeAsset>(
                    $"{SpaceWarpPlugin.ModGuid}/avcdialog/ui/avcdialog/avcdialog.uxml");

            var windowOptions = WindowOptions.Default;
            windowOptions.WindowId = "Space Warp AVC Dialog";
            windowOptions.Parent = ui.transform;
            var avcDialog = Window.Create(windowOptions, avcDialogUxml);

            var avcDialogController = avcDialog.gameObject.AddComponent<AvcDialogController>();
            avcDialogController.Module = VersionChecking.Instance;
        }

        InitializeUI();
    }

    /// <inheritdoc/>
    public override void PostInitializeModule()
    {
        ModuleLogger.LogInfo("Post Initializing UI");
        InitializeSettingsUI();
        InitializeSpaceWarpDetailsFoldout();
        ModListController.AddMainMenuItem();
    }

    /// <inheritdoc/>
    public override List<string> Prerequisites => ["SpaceWarp.VersionChecking"];

    private void InitializeUI()
    {
        ConfigurationManager = (ConfigManager)Chainloader
            .PluginInfos[ConfigManager.GUID]
            .Instance;

        var ui = new GameObject("Space Warp UI");
        ui.Persist();
        ui.SetActive(true);

        var modListUxml = AssetManager.GetAsset<VisualTreeAsset>(
            $"{SpaceWarpPlugin.ModGuid}/modlist/ui/modlist/modlist.uxml"
        );
        var modListOptions = WindowOptions.Default;
        modListOptions.WindowId = "Space Warp Mod List";
        modListOptions.Parent = ui.transform;
        var modList = Window.Create(modListOptions, modListUxml);
        ModListController = modList.gameObject.AddComponent<ModListController>();

        var swConsoleUxml = AssetManager.GetAsset<VisualTreeAsset>(
            $"{SpaceWarpPlugin.ModGuid}/swconsole/ui/console/console.uxml"
        );
        var swConsoleOptions = WindowOptions.Default;
        swConsoleOptions.WindowId = "Space Warp AVC Dialog";
        swConsoleOptions.Parent = ui.transform;
        var swConsole = Window.Create(swConsoleOptions, swConsoleUxml);
        SpaceWarpConsole = swConsole.gameObject.AddComponent<SpaceWarpConsole>();
    }


    private static void InitializeSettingsUI()
    {
        GameObject settingsController = new("Space Warp Settings Controller");
        settingsController.Persist();
        settingsController.AddComponent<SettingsMenuController>();
        settingsController.SetActive(true);
    }

    private static void InitializeSpaceWarpDetailsFoldout()
    {
        VisualElement GenerateModulesText()
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
            websiteLink.classList.Add("link");
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

        API.UI.ModList.RegisterDetailsFoldoutGenerator(SpaceWarpPlugin.ModGuid, GenerateModulesText);
    }
}