using System;
using System.Collections;
using System.Reflection;
using System.Xml;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.API.Versions;
using UnityEngine.Networking;

namespace SpaceWarp.Modules;

[PublicAPI]
public class VersionChecking : SpaceWarpModule
{
    public override string Name => "SpaceWarp.VersionChecking";
    public ConfigValue<bool> ConfigFirstLaunch;
    public ConfigValue<bool> ConfigCheckVersions;
    public static VersionChecking Instance;
    private string _kspVersion;
    public override void LoadModule()
    {
        Instance = this;
        ConfigFirstLaunch = new ConfigValue<bool>(ModuleConfiguration.Bind("Main", "First Launch", true,
            "Set this to false to get the version check prompt next launch"));
        ConfigCheckVersions = new ConfigValue<bool>(ModuleConfiguration.Bind("Main", "Check Versions", false,
            "Set this to true to automatically check versions over the internet"));
    }

    public override void PreInitializeModule()
    {
        _kspVersion = typeof(VersionID).GetField("VERSION_TEXT", BindingFlags.Static | BindingFlags.Public)
            ?.GetValue(null) as string;
    }

    public override void InitializeModule()
    {

        if (ConfigCheckVersions.Value)
        {
            CheckVersions();
        }
        CheckKspVersions();
    }

    public override void PostInitializeModule()
    {
        ConfigFirstLaunch.Value = false;
    }

    public void ClearVersions()
    {
        foreach (var plugin in PluginList.AllPlugins)
        {
            plugin.Outdated = false;
        }
    }
    public void CheckVersions()
    {
        ClearVersions();
        foreach (var plugin in PluginList.AllEnabledAndActivePlugins)
        {
            if (plugin.SWInfo.VersionCheck != null)
            {
                SpaceWarpPlugin.Instance.StartCoroutine(CheckVersion(plugin.Guid, plugin));
            }
        }

        foreach (var info in PluginList.AllDisabledPlugins)
        {
            if (info.SWInfo.VersionCheck != null)
            {
                SpaceWarpPlugin.Instance.StartCoroutine(info.Guid, info);
            }
        }
    }
    private IEnumerator CheckVersion(string guid, SpaceWarpPluginDescriptor info)
    {
        var www = UnityWebRequest.Get(info.SWInfo.VersionCheck);
        yield return www.SendWebRequest();
        
        if (www.result != UnityWebRequest.Result.Success)
        {
            ModuleLogger.LogInfo($"Unable to check version for {guid} due to error {www.error}");
        }
        else
        {
            var isOutdated = false;
            var unsupported = false;
            SupportedVersionsInfo newKSP2Versions = null;
            var results = www.downloadHandler.text;
            try
            {
                if (info.SWInfo.Spec >= SpecVersion.V2_0)
                {
                    isOutdated = CheckSemanticVersion(guid, info.SWInfo.Version, results, out unsupported, out newKSP2Versions);
                }
                else
                {
                    isOutdated = info.SWInfo.VersionCheckType switch
                    {
                        VersionCheckType.SwInfo => CheckJsonVersion(guid, info.SWInfo.Version, results, out unsupported, out newKSP2Versions),
                        VersionCheckType.Csproj => CheckCsprojVersion(guid, info.SWInfo.Version, results, out unsupported, out newKSP2Versions),
                        _ => throw new ArgumentOutOfRangeException(nameof(info), "Invalid version_check_type")
                    };
                }
            }
            catch (Exception e)
            {
                ModuleLogger.LogError($"Unable to check version for {guid} due to error {e}");
            }
            
            info.Outdated = isOutdated;
            info.Unsupported = unsupported;
            if (isOutdated)
            {
                ModuleLogger.LogWarning($"{guid} is outdated");
            }
            if (unsupported)
            {
                ModuleLogger.LogWarning($"{guid} is unsupported");
                info.SWInfo.SupportedKsp2Versions = newKSP2Versions;
            }
        }
    }

    private bool CheckSemanticVersion(string guid, string version, string json, out bool unsupported, out SupportedVersionsInfo checkVersions)
    {
        var checkInfo = JsonConvert.DeserializeObject<ModInfo>(json);
        var semverOne = new SemanticVersion(version);
        var semverTwo = new SemanticVersion(checkInfo.Version);
        unsupported = false;
        checkVersions = null;
        if (semverOne != semverTwo) return semverOne < semverTwo;
        if (checkInfo.SupportedKsp2Versions.IsSupported(_kspVersion)) return false;
        unsupported = true;
        checkVersions = checkInfo.SupportedKsp2Versions;
        return false;

    }

    private bool CheckJsonVersion(string guid, string version, string json, out bool unsupported, out SupportedVersionsInfo checkVersions)
    {
        var checkInfo = JsonConvert.DeserializeObject<ModInfo>(json);
        unsupported = false;
        checkVersions = null;
        if (version != checkInfo.Version) return VersionUtility.IsOlderThan(version, checkInfo.Version);
        if (checkInfo.SupportedKsp2Versions.IsSupported(_kspVersion)) return false;
        unsupported = true;
        checkVersions = checkInfo.SupportedKsp2Versions;
        return false;
    }

    private bool CheckCsprojVersion(string guid, string version, string csproj, out bool unsupported, out SupportedVersionsInfo checkVersions)
    {
        var document = new XmlDocument();
        document.LoadXml(csproj);

        var ksp2VersionMin = document.GetElementsByTagName("Ksp2VersionMin")[0]?.InnerText
            ?? SupportedVersionsInfo.DefaultMin;
        var ksp2VersionMax = document.GetElementsByTagName("Ksp2VersionMax")[0]?.InnerText
            ?? SupportedVersionsInfo.DefaultMax;
        checkVersions = new SupportedVersionsInfo()
        {
            Max = ksp2VersionMax,
            Min = ksp2VersionMin
        };
        unsupported = false;

        var checkVersionTags = document.GetElementsByTagName("Version");
        var checkVersion = checkVersionTags[0]?.InnerText;
        if (checkVersion == null || checkVersionTags.Count != 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(csproj),
                "There must be exactly 1 Version tag in the checked .csproj"
            );
        }

        if (version != checkVersion) return VersionUtility.IsOlderThan(version, checkVersion);
        if (checkVersions.IsSupported(_kspVersion)) return false;
        unsupported = true;
        return false;
    }

    internal void CheckKspVersions()
    {
        foreach (var plugin in PluginList.AllEnabledAndActivePlugins)
        {
            // CheckModKspVersion(plugin.Info.Metadata.GUID, plugin.SpaceWarpMetadata, kspVersion);
            CheckModKspVersion(plugin.Guid, plugin, _kspVersion);
        }

        foreach (var info in PluginList.AllDisabledPlugins)
        {
            CheckModKspVersion(info.Guid, info, _kspVersion);
        }
    }

    private void CheckModKspVersion(string guid, SpaceWarpPluginDescriptor info, string kspVersion)
    {
        var unsupported = true;
        try
        {
            unsupported = !info.SWInfo.SupportedKsp2Versions.IsSupported(kspVersion);
        }
        catch (Exception e)
        {
            ModuleLogger.LogError($"Unable to check KSP version for {guid} due to error {e}");
        }

        if (unsupported)
        {
            ModuleLogger.LogWarning($"{guid} is unsupported");
        }

        info.Unsupported = info.Unsupported || unsupported;
    }
}