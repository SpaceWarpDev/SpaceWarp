global using UnityObject = UnityEngine.Object;
global using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KSP.Messages;
using SpaceWarp.API.Game.Messages;
using SpaceWarp.API.Mods;
using SpaceWarp.UI;
using UnityEngine;

namespace SpaceWarp;

[BepInDependency(ConfigurationManager.ConfigurationManager.GUID,ConfigurationManager.ConfigurationManager.Version)]
[BepInPlugin(ModGuid, ModName, ModVer)]
public sealed class SpaceWarpPlugin : BaseSpaceWarpPlugin
{
    public const string ModGuid = "com.github.x606.spacewarp";
    public const string ModName = "Space Warp";
    public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    internal ConfigEntry<Color> configErrorColor;
    internal ConfigEntry<Color> configWarningColor;
    internal ConfigEntry<Color> configMessageColor;
    internal ConfigEntry<Color> configInfoColor;
    internal ConfigEntry<Color> configDebugColor;
    internal ConfigEntry<Color> configAllColor;
    internal ConfigEntry<bool> configShowConsoleButton;
    internal ConfigEntry<bool> configShowTimeStamps;

    internal new ManualLogSource Logger => base.Logger;

    public void Awake()
    {
        configErrorColor = Config.Bind("Debug Console", "Color Error", Color.red,
            "The color for log messages that have the level: Error/Fatal (bolded)");
        configWarningColor = Config.Bind("Debug Console", "Color Warning", Color.yellow,
            "The color for log messages that have the level: Warning");
        configMessageColor = Config.Bind("Debug Console", "Color Message", Color.white,
            "The color for log messages that have the level: Message");
        configInfoColor = Config.Bind("Debug Console", "Color Info", Color.cyan,
            "The color for log messages that have the level: Info");
        configDebugColor = Config.Bind("Debug Console", "Color Debug", Color.green,
            "The color for log messages that have the level: Debug");
        configAllColor = Config.Bind("Debug Console", "Color All", Color.magenta,
            "The color for log messages that have the level: All");
        configShowConsoleButton = Config.Bind("Debug Console", "Show Console Button", false,
                "Show console button in app.bar, requires restart");
        configShowTimeStamps = Config.Bind("Debug Console", "Show Timestamps", false, 
            "Show time stamps in debug console");

        BepInEx.Logging.Logger.Listeners.Add(new SpaceWarpConsoleLogListener());

        Harmony.CreateAndPatchAll(typeof(SpaceWarpPlugin).Assembly, ModGuid);
        
        SpaceWarpManager.Initialize(this);
    }


    public override void OnInitialized()
    {
        base.OnInitialized();
        
        Game.Messages.Subscribe(typeof(GameStateEnteredMessage), StateChanges.OnGameStateEntered,false,true);
        Game.Messages.Subscribe(typeof(GameStateLeftMessage), StateChanges.OnGameStateLeft,false,true);
        Game.Messages.Subscribe(typeof(GameStateChangedMessage), StateChanges.OnGameStateChanged,false,true);
        
        InitializeUI();
    }

    private void InitializeUI()
    {
        SpaceWarpManager.ConfigurationManager = (ConfigurationManager.ConfigurationManager)Chainloader.PluginInfos[global::ConfigurationManager.ConfigurationManager.GUID].Instance;
        GameObject modUIObject = new("Space Warp Mod UI");
        modUIObject.Persist();

        modUIObject.transform.SetParent(this.transform);
        SpaceWarpManager.ModListUI = modUIObject.AddComponent<ModListUI>();

        modUIObject.SetActive(true);

        GameObject consoleUIObject = new("Space Warp Console");
        consoleUIObject.Persist();
        consoleUIObject.transform.SetParent(Chainloader.ManagerObject.transform);
        consoleUIObject.AddComponent<SpaceWarpConsole>();
        consoleUIObject.SetActive(true);
        
        API.UI.MainMenu.RegisterMenuButton("mods", SpaceWarpManager.ModListUI.ToggleVisible);
    }
}