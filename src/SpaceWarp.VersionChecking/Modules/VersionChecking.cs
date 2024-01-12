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

/// <summary>
/// Module that handles version checking.
/// </summary>
[PublicAPI]
public class VersionChecking : SpaceWarpModule
{
    /// <inheritdoc />
    public override string Name => "SpaceWarp.VersionChecking";

    /// <summary>
    /// The config value for whether this is the first launch.
    /// </summary>
    public ConfigValue<bool> ConfigFirstLaunch;

    /// <summary>
    /// The config value for whether to check versions.
    /// </summary>
    public ConfigValue<bool> ConfigCheckVersions;

    /// <summary>
    /// The instance of the version checking module.
    /// </summary>
    public static VersionChecking Instance;

    private string _kspVersion;

    /// <inheritdoc />
    public override void LoadModule()
    {
        Instance = this;
        ConfigFirstLaunch = new ConfigValue<bool>(ModuleConfiguration.Bind("Main", "First Launch", true,
            "Set this to false to get the version check prompt next launch"));
        ConfigCheckVersions = new ConfigValue<bool>(ModuleConfiguration.Bind("Main", "Check Versions", false,
            "Set this to true to automatically check versions over the internet"));
    }

    /// <inheritdoc />
    public override void PreInitializeModule()
    {
        _kspVersion = typeof(VersionID)
            .GetField("VERSION_TEXT", BindingFlags.Static | BindingFlags.Public)
            ?.GetValue(null) as string;
    }

    /// <inheritdoc />
    public override void InitializeModule()
    {
        if (ConfigCheckVersions.Value)
        {
            CheckVersions();
        }

        CheckKspVersions();
    }

    /// <inheritdoc />
    public override void PostInitializeModule()
    {
        ConfigFirstLaunch.Value = false;
    }

    /// <summary>
    /// Clears the outdated flag on all plugins.
    /// </summary>
    public void ClearVersions()
    {
        foreach (var plugin in PluginList.AllPlugins)
        {
            plugin.Outdated = false;
        }
    }

    /// <summary>
    /// Checks the versions of all plugins.
    /// </summary>
    public void CheckVersions()
    {
        var uiModule = (SpaceWarpModule)AppDomain.CurrentDomain.GetAssemblies()
            .First(assembly => assembly.FullName.StartsWith("SpaceWarp.UI"))
            .GetTypes()
            .First(type => type.FullName == "SpaceWarp.Modules.UI")
            .GetField("Instance", BindingFlags.Static | BindingFlags.NonPublic)
            ?.GetValue(null);

        var modListControllerField = uiModule
            ?.GetType()
            .GetField("ModListController", BindingFlags.Instance | BindingFlags.NonPublic);

        var versionCheckCallback = (string guid, bool isOutdated) =>
        {
            var modListController = modListControllerField?.GetValue(uiModule);

            if (modListController == null)
            {
                return false;
            }

            modListControllerField
                .FieldType
                .GetMethod("UpdateOutdated", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(modListController, new object[] { guid, isOutdated });

            return true;
        };

        ClearVersions();
        foreach (var plugin in PluginList.AllEnabledAndActivePlugins)
        {
            if (plugin.SWInfo.VersionCheck != null)
            {
                SpaceWarpPlugin.Instance.StartCoroutine(CheckVersion(plugin.Guid, plugin, versionCheckCallback));
            }
        }

        foreach (var info in PluginList.AllDisabledPlugins)
        {
            if (info.SWInfo.VersionCheck != null)
            {
                SpaceWarpPlugin.Instance.StartCoroutine(CheckVersion(info.Guid, info, versionCheckCallback));
            }
        }
    }

    private IEnumerator CheckVersion(string guid, SpaceWarpPluginDescriptor info, Func<string, bool, bool> callback)
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
            SupportedVersionsInfo newKsp2Versions = null;
            var results = www.downloadHandler.text;
            try
            {
                if (info.SWInfo.Spec >= SpecVersion.V2_0)
                {
                    isOutdated = CheckSemanticVersion(guid, info.SWInfo.Version, results, out unsupported,
                        out newKsp2Versions);
                }
                // TODO: Remove this in 2.0
#pragma warning disable CS0618 // Type or member is obsolete
                else
                {


                    isOutdated = info.SWInfo.VersionCheckType switch
                    {
                        VersionCheckType.SwInfo => CheckJsonVersion(guid, info.SWInfo.Version, results, out unsupported,
                            out newKsp2Versions),
                        VersionCheckType.Csproj => CheckCsprojVersion(guid, info.SWInfo.Version, results,
                            out unsupported, out newKsp2Versions),
                        _ => throw new ArgumentOutOfRangeException(nameof(info), "Invalid version_check_type")
                    };
                }
#pragma warning restore CS0618 // Type or member is obsolete
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
                info.SWInfo.SupportedKsp2Versions = newKsp2Versions;
            }

            while (!callback(guid, isOutdated))
            {
                yield return new WaitForUpdate();
            }
        }
    }

    private bool CheckSemanticVersion(string guid, string version, string json, out bool unsupported,
        out SupportedVersionsInfo checkVersions)
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

    private bool CheckJsonVersion(string guid, string version, string json, out bool unsupported,
        out SupportedVersionsInfo checkVersions)
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

    private bool CheckCsprojVersion(string guid, string version, string csproj, out bool unsupported,
        out SupportedVersionsInfo checkVersions)
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