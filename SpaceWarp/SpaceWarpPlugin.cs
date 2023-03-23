global using UnityObject = UnityEngine.Object;
global using System.Linq;
using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using I2.Loc;
using KSP.Messages;
using Newtonsoft.Json;
using SpaceWarp.API.Game.Messages;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.API.UI;
using SpaceWarp.API.Versions;
using SpaceWarp.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace SpaceWarp;

[BepInDependency(ConfigurationManager.ConfigurationManager.GUID, ConfigurationManager.ConfigurationManager.Version)]
[BepInPlugin(ModGuid, ModName, ModVer)]
public sealed class SpaceWarpPlugin : BaseSpaceWarpPlugin
{
    public const string ModGuid = "com.github.x606.spacewarp";
    public const string ModName = "Space Warp";
    public const string ModVer = MyPluginInfo.PLUGIN_VERSION;
    internal ConfigEntry<Color> ConfigAllColor;
    internal ConfigEntry<bool> ConfigCheckVersions;
    internal ConfigEntry<Color> ConfigDebugColor;
    internal ConfigEntry<int> ConfigDebugMessageLimit;

    internal ConfigEntry<Color> ConfigErrorColor;
    private ConfigEntry<bool> _configFirstLaunch;
    internal ConfigEntry<Color> ConfigInfoColor;
    internal ConfigEntry<Color> ConfigMessageColor;
    internal ConfigEntry<bool> ConfigShowConsoleButton;
    internal ConfigEntry<bool> ConfigShowTimeStamps;
    internal ConfigEntry<string> ConfigTimeStampFormat;
    internal ConfigEntry<Color> ConfigWarningColor;
    private string _kspVersion;

    internal new ManualLogSource Logger => base.Logger;

    public void Awake()
    {
        _kspVersion = typeof(VersionID).GetField("VERSION_TEXT", BindingFlags.Static | BindingFlags.Public)
            ?.GetValue(null) as string;
        ConfigErrorColor = Config.Bind("Debug Console", "Color Error", Color.red,
            "The color for log messages that have the level: Error/Fatal (bolded)");
        ConfigWarningColor = Config.Bind("Debug Console", "Color Warning", Color.yellow,
            "The color for log messages that have the level: Warning");
        ConfigMessageColor = Config.Bind("Debug Console", "Color Message", Color.white,
            "The color for log messages that have the level: Message");
        ConfigInfoColor = Config.Bind("Debug Console", "Color Info", Color.cyan,
            "The color for log messages that have the level: Info");
        ConfigDebugColor = Config.Bind("Debug Console", "Color Debug", Color.green,
            "The color for log messages that have the level: Debug");
        ConfigAllColor = Config.Bind("Debug Console", "Color All", Color.magenta,
            "The color for log messages that have the level: All");
        ConfigShowConsoleButton = Config.Bind("Debug Console", "Show Console Button", false,
            "Show console button in app.bar, requires restart");
        ConfigShowTimeStamps = Config.Bind("Debug Console", "Show Timestamps", true,
            "Show time stamps in debug console");
        ConfigTimeStampFormat = Config.Bind("Debug Console", "Timestamp Format", "HH:mm:ss.fff",
            "The format for the timestamps in the debug console.");
        ConfigDebugMessageLimit = Config.Bind("Debug Console", "Message Limit", 1000,
            "The maximum number of messages to keep in the debug console.");
        _configFirstLaunch = Config.Bind("Version Checking", "First Launch", true,
            "Whether or not this is the first launch of space warp, used to show the version checking prompt to the user.");
        ConfigCheckVersions = Config.Bind("Version Checking", "Check Versions", false,
            "Whether or not Space Warp should check mod versions using their swinfo.json files");

        BepInEx.Logging.Logger.Listeners.Add(new SpaceWarpConsoleLogListener(this));

        Harmony.CreateAndPatchAll(typeof(SpaceWarpPlugin).Assembly, ModGuid);

        SpaceWarpManager.Initialize(this);
    }


    public override void OnInitialized()
    {
        base.OnInitialized();

        Game.Messages.Subscribe(typeof(GameStateEnteredMessage), StateChanges.OnGameStateEntered, false, true);
        Game.Messages.Subscribe(typeof(GameStateLeftMessage), StateChanges.OnGameStateLeft, false, true);
        Game.Messages.Subscribe(typeof(GameStateChangedMessage), StateChanges.OnGameStateChanged, false, true);

        InitializeUI();
        if (_configFirstLaunch.Value)
        {
            _configFirstLaunch.Value = false;
            // Generate a prompt for whether or not space warp should check mod versions
            var o = new GameObject();
            var prompt = o.AddComponent<VersionCheckPrompt>();
            prompt.spaceWarpPlugin = this;
        }

        if (ConfigCheckVersions.Value)
            CheckVersions();
        else
            ClearVersions();

        SpaceWarpManager.CheckKspVersions();
    }

    public void ClearVersions()
    {
        foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins)
            SpaceWarpManager.ModsOutdated[plugin.SpaceWarpMetadata.ModID] = false;

        foreach (var info in SpaceWarpManager.NonSpaceWarpInfos) SpaceWarpManager.ModsOutdated[info.ModID] = false;
    }

    public void CheckVersions()
    {
        ClearVersions();
        foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins)
            if (plugin.SpaceWarpMetadata.VersionCheck != null)
                StartCoroutine(CheckVersion(plugin.SpaceWarpMetadata));

        foreach (var info in SpaceWarpManager.NonSpaceWarpInfos)
            if (info.VersionCheck != null)
                StartCoroutine(CheckVersion(info));
    }

    private static bool OlderThan(string currentVersion, string onlineVersion)
    {
        return VersionUtility.CompareSemanticVersionStrings(currentVersion, onlineVersion) < 0;
    }

    private IEnumerator CheckVersion(ModInfo pluginInfo)
    {
        var www = UnityWebRequest.Get(pluginInfo.VersionCheck);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Logger.LogInfo($"Unable to check version for {pluginInfo.ModID} due to error {www.error}");
        }
        else
        {
            var results = www.downloadHandler.text;
            try
            {
                var checkInfo = JsonConvert.DeserializeObject<ModInfo>(results);
                if (!checkInfo.SupportedKsp2Versions.IsSupported(_kspVersion)) yield break;
                SpaceWarpManager.ModsOutdated[pluginInfo.ModID] = OlderThan(pluginInfo.Version, checkInfo.Version);
            }
            catch (Exception e)
            {
                Logger.LogError($"Unable to check version for {pluginInfo.ModID} due to error {e}");
            }
        }
    }

    private void InitializeUI()
    {
        SpaceWarpManager.ConfigurationManager =
            (ConfigurationManager.ConfigurationManager)Chainloader
                .PluginInfos[ConfigurationManager.ConfigurationManager.GUID].Instance;
        GameObject modUIObject = new("Space Warp Mod UI");
        modUIObject.Persist();

        modUIObject.transform.SetParent(transform);
        SpaceWarpManager.ModListUI = modUIObject.AddComponent<ModListUI>();

        modUIObject.SetActive(true);

        GameObject consoleUIObject = new("Space Warp Console");
        consoleUIObject.Persist();
        consoleUIObject.transform.SetParent(Chainloader.ManagerObject.transform);
        consoleUIObject.AddComponent<SpaceWarpConsole>();
        consoleUIObject.SetActive(true);

        MainMenu.RegisterLocalizedMenuButton("SpaceWarp/Mods", SpaceWarpManager.ModListUI.ToggleVisible);
    }
}