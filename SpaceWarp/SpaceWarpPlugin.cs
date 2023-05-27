global using UnityObject = UnityEngine.Object;
global using System.Linq;
using System;
using System.Collections;
using System.Reflection;
using System.Xml;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KSP.Messages;
using UitkForKsp2.API;
using Newtonsoft.Json;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Game.Messages;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.API.Versions;
using SpaceWarp.UI;
using SpaceWarp.UI.Debug;
using SpaceWarp.UI.ModList;
using UitkForKsp2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace SpaceWarp;

[BepInDependency(ConfigurationManager.ConfigurationManager.GUID, ConfigurationManager.ConfigurationManager.Version)]
[BepInDependency(UitkForKsp2Plugin.ModGuid, UitkForKsp2Plugin.ModVer)]
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
    internal ConfigEntry<Color> ConfigAutoScrollEnabledColor;
    private string _kspVersion;

    internal new static ManualLogSource Logger;

    public SpaceWarpPlugin()
    {
        Logger = base.Logger;
    }

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

        SpaceWarpManager.InitializeSpaceWarpsLoadingActions();

        SpaceWarpManager.Initialize(this);
    }


    public override void OnInitialized()
    {
        base.OnInitialized();

        Game.Messages.Subscribe(typeof(GameStateEnteredMessage), StateChanges.OnGameStateEntered, false, true);
        Game.Messages.Subscribe(typeof(GameStateLeftMessage), StateChanges.OnGameStateLeft, false, true);
        Game.Messages.Subscribe(typeof(GameStateChangedMessage), StateChanges.OnGameStateChanged, false, true);

        if (_configFirstLaunch.Value)
        {
            _configFirstLaunch.Value = false;
            // Generate a prompt for whether or not space warp should check mod versions
            var o = new GameObject();
            var prompt = o.AddComponent<VersionCheckPrompt>();
            prompt.spaceWarpPlugin = this;
        }

        SpaceWarpManager.CheckKspVersions();

        InitializeUI();

        if (ConfigCheckVersions.Value)
        {
            CheckVersions();
        }
        else
        {
            ClearVersions();
        }
    }

    public void ClearVersions()
    {
        foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins)
        {
            SpaceWarpManager.ModsOutdated[plugin.Info.Metadata.GUID] = false;
        }

        foreach (var info in SpaceWarpManager.NonSpaceWarpInfos)
        {
            SpaceWarpManager.ModsOutdated[info.Item1.Info.Metadata.GUID] = false;
        }

        foreach (var info in SpaceWarpManager.DisabledInfoPlugins)
        {
            SpaceWarpManager.ModsOutdated[info.Item1.Metadata.GUID] = false;
        }
    }

    public void CheckVersions()
    {
        ClearVersions();
        foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins)
        {
            if (plugin.SpaceWarpMetadata.VersionCheck != null)
            {
                StartCoroutine(CheckVersion(plugin.Info.Metadata.GUID, plugin.SpaceWarpMetadata));
            }
        }

        foreach (var info in SpaceWarpManager.NonSpaceWarpInfos)
        {
            if (info.Item2.VersionCheck != null)
            {
                StartCoroutine(CheckVersion(info.Item1.Info.Metadata.GUID, info.Item2));
            }
        }

        foreach (var info in SpaceWarpManager.DisabledInfoPlugins)
        {
            if (info.Item2.VersionCheck != null)
            {
                StartCoroutine(CheckVersion(info.Item1.Metadata.GUID, info.Item2));
            }
        }
    }

    private IEnumerator CheckVersion(string guid, ModInfo modInfo)
    {
        var www = UnityWebRequest.Get(modInfo.VersionCheck);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Logger.LogInfo($"Unable to check version for {guid} due to error {www.error}");
        }
        else
        {
            var isOutdated = false;
            var results = www.downloadHandler.text;
            try
            {
                isOutdated = modInfo.VersionCheckType switch
                {
                    VersionCheckType.SwInfo => CheckJsonVersion(guid, modInfo.Version, results),
                    VersionCheckType.Csproj => CheckCsprojVersion(guid, modInfo.Version, results),
                    _ => throw new ArgumentOutOfRangeException(nameof(modInfo), "Invalid version_check_type")
                };
            }
            catch (Exception e)
            {
                Logger.LogError($"Unable to check version for {guid} due to error {e}");
            }

            SpaceWarpManager.ModListController.UpdateOutdated(guid, isOutdated);
        }
    }

    private bool CheckJsonVersion(string guid, string version, string json)
    {
        var checkInfo = JsonConvert.DeserializeObject<ModInfo>(json);
        if (!checkInfo.SupportedKsp2Versions.IsSupported(_kspVersion))
        {
            return false;
        }

        var isOutdated = VersionUtility.IsOlderThan(version, checkInfo.Version);
        SpaceWarpManager.ModsOutdated[guid] = isOutdated;
        return isOutdated;
    }

    private bool CheckCsprojVersion(string guid, string version, string csproj)
    {
        var document = new XmlDocument();
        document.LoadXml(csproj);

        var ksp2VersionMin = document.GetElementsByTagName("Ksp2VersionMin")[0]?.InnerText
            ?? SupportedVersionsInfo.DefaultMin;
        var ksp2VersionMax = document.GetElementsByTagName("Ksp2VersionMax")[0]?.InnerText
            ?? SupportedVersionsInfo.DefaultMax;

        if (!VersionUtility.IsSupported(_kspVersion, ksp2VersionMin, ksp2VersionMax))
        {
            return false;
        }

        var checkVersionTags = document.GetElementsByTagName("Version");
        var checkVersion = checkVersionTags[0]?.InnerText;
        if (checkVersion == null || checkVersionTags.Count != 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(csproj),
                "There must be exactly 1 Version tag in the checked .csproj"
            );
        }

        var isOutdated = VersionUtility.IsOlderThan(version, checkVersion);
        SpaceWarpManager.ModsOutdated[guid] = isOutdated;
        return isOutdated;
    }

    private void InitializeUI()
    {
        SpaceWarpManager.ConfigurationManager =
            (ConfigurationManager.ConfigurationManager)Chainloader
                .PluginInfos[ConfigurationManager.ConfigurationManager.GUID].Instance;

        var modListUxml = AssetManager.GetAsset<VisualTreeAsset>($"spacewarp/modlist/modlist.uxml");
        var modList = Window.CreateFromUxml(modListUxml, "Space Warp Mod List", transform, true);
        
        var swConsoleUxml = AssetManager.GetAsset<VisualTreeAsset>($"spacewarp/swconsole/swconsole.uxml");
        var swConsole = Window.CreateFromUxml(swConsoleUxml, "Space Warp Console", transform, true);
        
        SpaceWarpManager.ModListController = modList.gameObject.AddComponent<ModListController>();
        modList.gameObject.Persist();

        SpaceWarpManager.SpaceWarpConsole = swConsole.gameObject.AddComponent<SpaceWarpConsole>();
        swConsole.gameObject.Persist();
    }
}