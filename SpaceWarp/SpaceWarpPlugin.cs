global using UnityObject = UnityEngine.Object;
global using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KSP.Messages;
using Newtonsoft.Json;
using SpaceWarp.API.Game.Messages;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.UI;
using UnityEngine;
using UnityEngine.Networking;

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
    internal ConfigEntry<string> configTimeStampFormat;
    internal ConfigEntry<int> configDebugMessageLimit;
    internal ConfigEntry<bool> configFirstLaunch;
    internal ConfigEntry<bool> configCheckVersions;


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
        configShowTimeStamps = Config.Bind("Debug Console", "Show Timestamps", true, 
            "Show time stamps in debug console");
        configTimeStampFormat = Config.Bind("Debug Console", "Timestamp Format", "HH:mm:ss.fff",
            "The format for the timestamps in the debug console.");
        configDebugMessageLimit = Config.Bind("Debug Console", "Message Limit", 1000,
            "The maximum number of messages to keep in the debug console.");
        configFirstLaunch = Config.Bind("Version Checking", "First Launch", true,
            "Whether or not this is the first launch of space warp, used to show the version checking prompt to the user.");
        configCheckVersions = Config.Bind("Version Checking", "Check Versions", false,
            "Whether or not Space Warp should check mod versions using their swinfo.json files");
        
        BepInEx.Logging.Logger.Listeners.Add(new SpaceWarpConsoleLogListener(this));

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

    public override void OnPostInitialized()
    {
        base.OnPostInitialized();
        if (configFirstLaunch.Value)
        {
            configFirstLaunch.Value = false;
            // Generate a prompt for whether or not space warp should check mod versions
            GameObject o = new GameObject();
            VersionCheckPrompt prompt = o.AddComponent<VersionCheckPrompt>();
            prompt.spaceWarpPlugin = this;
        }

        if (configCheckVersions.Value)
        {
            CheckVersions();
        }
        else
        {
            
        }
    }

    public void ClearVersions()
    {
        foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins)
        {
            SpaceWarpManager.ModsOutdated[plugin.SpaceWarpMetadata.ModID] = false;
        }
    }
    public void CheckVersions()
    {
        ClearVersions();
        foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins)
        {
            if (plugin.SpaceWarpMetadata.VersionCheck != null)
            {
                StartCoroutine(CheckVersion(plugin.SpaceWarpMetadata));
            }
        }
    }

    static bool OlderThan(string currentVersion, string onlineVersion)
    {
        try
        {
            var currentList = currentVersion.Split('.');
            var onlineList = onlineVersion.Split('.');
            var minLength = Math.Min(currentList.Length, onlineList.Length);
            for (var i = 0; i < minLength; i++)
            {
                if (int.Parse(currentList[i]) < int.Parse(onlineList[i]))
                {
                    return true;
                }

                if (int.Parse(currentList[i]) > int.Parse(onlineList[i]))
                {
                    return false;
                }
            }

            return onlineList.Length > currentList.Length;
        } catch (Exception e)
        {
            Debug.Log(e);
            return false;
        }
    }
    IEnumerator CheckVersion(ModInfo pluginInfo)
    {
        var www = new UnityWebRequest(pluginInfo.VersionCheck);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Logger.LogInfo($"Unable to check version for {pluginInfo.ModID} due to error {www.error}");
        }
        else
        {
            var results = www.downloadHandler.text;
            var checkInfo = JsonConvert.DeserializeObject<ModInfo>(results);
            SpaceWarpManager.ModsOutdated[pluginInfo.ModID] = OlderThan(pluginInfo.Version, checkInfo.Version);
        }
    }
    
    private void InitializeUI()
    {
        SpaceWarpManager.ConfigurationManager = (ConfigurationManager.ConfigurationManager)Chainloader.PluginInfos[global::ConfigurationManager.ConfigurationManager.GUID].Instance;
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
        
        API.UI.MainMenu.RegisterMenuButton("mods", SpaceWarpManager.ModListUI.ToggleVisible);
    }
}