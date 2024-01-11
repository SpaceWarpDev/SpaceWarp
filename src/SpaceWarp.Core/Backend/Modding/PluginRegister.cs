using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using Newtonsoft.Json;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Logging;
using SpaceWarp.API.Mods;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.API.Versions;
using SpaceWarp.Patcher;
using SpaceWarp.Patcher.API;
using SpaceWarp.Patcher.Patches;

namespace SpaceWarp.Backend.Modding;

internal static class PluginRegister
{
    public static void RegisterAllMods()
    {
        RegisterAllBepInExMods();
        GetDisabledPlugins();
        RegisterAllCodelessMods();
        RegisterAllKspMods();
        RegisterAllErroredMods();
        DisableMods();
    }

    private static readonly ILogger Logger = SpaceWarpPlugin.Instance.SWLogger;

    private static ModInfo BepInExToSWInfo(PluginInfo plugin)
    {
        var newInfo = new ModInfo
        {
            Spec = SpecVersion.V2_0,
            ModID = plugin.Metadata.GUID,
            Name = plugin.Metadata.Name,
            Author = "<unknown>",
            Description = "<unknown>",
            Source = "<unknown>",
            Version = plugin.Metadata.Version.ToString(),
            Dependencies = plugin.Dependencies.Select(x => new DependencyInfo
            {
                ID = x.DependencyGUID,
                Version = new SupportedVersionsInfo
                {
                    Min = x.MinimumVersion.ToString(),
                    Max = "*"
                }
            }).ToList(),
            SupportedKsp2Versions = new SupportedVersionsInfo
            {
                Min = "*",
                Max = "*"
            },
            VersionCheck = null,
            Conflicts = plugin.Incompatibilities.Select(x => new DependencyInfo
            {
                ID = x.IncompatibilityGUID,
                Version = new SupportedVersionsInfo
                {
                    Min = "*",
                    Max = "*"
                }
            }).ToList()
        };
        return newInfo;
    }

    private static bool AssertFolderPath(BaseSpaceWarpPlugin plugin, string folderPath)
    {
        if (Path.GetFileName(folderPath) != "plugins") return true;

        Logger.LogError(
            $"Found Space Warp mod {plugin.Info.Metadata.Name} in the BepInEx/plugins directory. This mod will " +
            $"not be initialized."
        );

        var descriptor = new SpaceWarpPluginDescriptor(
            plugin,
            plugin.Info.Metadata.GUID,
            plugin.Info.Metadata.Name,
            BepInExToSWInfo(plugin.Info),
            new DirectoryInfo(folderPath),
            false,
            new BepInExConfigFile(plugin.Config)
        );
        PluginList.NoteBadDirectoryError(descriptor);

        return false;
    }

    private static bool AssertModInfoExistence(BaseSpaceWarpPlugin plugin, string modInfoPath, string folderPath)
    {
        if (File.Exists(modInfoPath))
        {
            return true;
        }

        Logger.LogError(
            $"Found Space Warp plugin {plugin.Info.Metadata.Name} without a swinfo.json in its folder. This mod " +
            $"will not be initialized."
        );

        PluginList.NoteMissingSwinfoError(new SpaceWarpPluginDescriptor(plugin,
            plugin.Info.Metadata.GUID,
            plugin.Info.Metadata.Name,
            BepInExToSWInfo(plugin.Info),
            new DirectoryInfo(folderPath))
        );

        return false;
    }

    private static bool TryReadModInfo(
        BaseUnityPlugin plugin,
        string modInfoPath,
        string folderPath,
        out ModInfo metadata
    )
    {
        try
        {
            metadata = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(modInfoPath));
        }
        catch
        {
            Logger.LogError(
                $"Error reading metadata for spacewarp plugin {plugin.Info.Metadata.Name}. This mod will not be initialized");
            PluginList.NoteMissingSwinfoError(new SpaceWarpPluginDescriptor(
                plugin as BaseSpaceWarpPlugin,
                plugin.Info.Metadata.GUID,
                plugin.Info.Metadata.Name,
                BepInExToSWInfo(plugin.Info),
                new DirectoryInfo(folderPath)
            ));
            metadata = null;
            return false;
        }

        return true;
    }

    private static bool AssertSpecificationCompliance(
        SpaceWarpPluginDescriptor descriptor,
        BaseUnityPlugin plugin,
        ModInfo metadata
    ) => metadata.Spec < SpecVersion.V1_3 || AssertSpecVersion13Compliance(
        descriptor,
        plugin,
        metadata
    );

    private static bool AssertSpecVersion13Compliance(
        SpaceWarpPluginDescriptor descriptor,
        BaseUnityPlugin plugin,
        ModInfo metadata
    ) => AssertMatchingModID(descriptor, plugin, metadata) &&
         AssertMatchingVersions(descriptor, plugin, metadata) &&
         AssertAllDependenciesAreSpecified(descriptor, plugin, metadata);

    private static bool AssertAllDependenciesAreSpecified(
        SpaceWarpPluginDescriptor descriptor,
        BaseUnityPlugin plugin,
        ModInfo metadata
    ) => plugin.Info.Dependencies.Aggregate(
        true,
        (current, dep) => current && AssertDependencyIsSpecified(plugin, descriptor, dep, metadata)
    );

    private static bool AssertDependencyIsSpecified(
        BaseUnityPlugin plugin,
        SpaceWarpPluginDescriptor descriptor,
        BepInDependency dep,
        ModInfo metadata
    )
    {
        if (metadata.Dependencies.Any(x => string.Equals(
                x.ID,
                dep.DependencyGUID,
                StringComparison.InvariantCultureIgnoreCase
            )))
        {
            return true;
        }

        Logger.LogError(
            $"Found Space Warp Plugin {plugin.Info.Metadata.Name} that has an unspecified swinfo dependency found " +
            $"in its BepInDependencies: {dep.DependencyGUID}"
        );
        PluginList.NoteUnspecifiedDependencyError(descriptor, dep.DependencyGUID);
        metadata.Dependencies.Add(new DependencyInfo
        {
            ID = dep.DependencyGUID,
            Version = new SupportedVersionsInfo
            {
                Min = dep.MinimumVersion.ToString(),
                Max = "*"
            }
        });
        return false;
    }

    private static string ClearPrerelease(string version)
    {
        var semver = new SemanticVersion(version);
        return $"{semver.Major}.{semver.Minor}.{semver.Patch}" +
               $"{(semver.VersionNumbers.Count > 3 ? $".{semver.VersionNumbers[3]}" : "")}";
    }

    private static bool AssertMatchingVersions(
        SpaceWarpPluginDescriptor descriptor,
        BaseUnityPlugin plugin,
        ModInfo metadata
    )
    {
        if (new Version(ClearPrerelease(metadata.Version)) == plugin.Info.Metadata.Version) return true;

        Logger.LogError(
            $"Found Space Warp plugin {plugin.Info.Metadata.Name} that's swinfo version ({metadata.Version}) does " +
            $"not match the plugin version ({plugin.Info.Metadata.Version}), this mod will not be initialized"
        );
        PluginList.NoteMismatchedVersionError(descriptor);
        return false;
    }

    private static bool AssertMatchingModID(
        SpaceWarpPluginDescriptor descriptor,
        BaseUnityPlugin plugin,
        ModInfo metadata
    )
    {
        var modID = metadata.ModID;
        if (modID == plugin.Info.Metadata.GUID) return true;

        Logger.LogError(
            $"Found SpaceWarp plugin {plugin.Info.Metadata.Name} that has a swinfo.json file with spec version >=" +
            $" 1.3 whose ModID is not the same as the plugin's GUID. This mod will not be initialized."
        );
        PluginList.NoteBadIDError(descriptor);
        return false;
    }

    private static void RegisterSingleSpaceWarpPlugin(BaseSpaceWarpPlugin plugin)
    {
        var folderPath = Path.GetDirectoryName(plugin.Info.Location);

#pragma warning disable CS0618 // Type or member is obsolete
        // TODO: Remove this in 2.0
        plugin.PluginFolderPath = folderPath;
#pragma warning restore CS0618 // Type or member is obsolete

        if (!AssertFolderPath(plugin, folderPath))
        {
            return;
        }

        var modInfoPath = Path.Combine(folderPath!, "swinfo.json");

        if (!AssertModInfoExistence(plugin, modInfoPath, folderPath))
        {
            return;
        }

        if (!TryReadModInfo(plugin, modInfoPath, folderPath, out var metadata))
        {
            return;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        // TODO: Remove this in 2.0
        plugin.SpaceWarpMetadata = metadata;
#pragma warning restore CS0618 // Type or member is obsolete

        var directoryInfo = new FileInfo(modInfoPath).Directory;
        var descriptor = new SpaceWarpPluginDescriptor(
            plugin,
            metadata.Spec != SpecVersion.V1_2 ? metadata.ModID : plugin.Info.Metadata.GUID,
            metadata.Name,
            metadata,
            directoryInfo,
            true,
            new BepInExConfigFile(plugin.Config)
        );
        descriptor.Plugin!.SWMetadata = descriptor;
        if (!AssertSpecificationCompliance(descriptor, plugin, metadata))
        {
            return;
        }

        PluginList.RegisterPlugin(descriptor);
    }

    private static void RegisterSingleBepInExPlugin(BaseUnityPlugin plugin)
    {
        if (PluginList.AllPlugins.Any(x => x.Plugin is BaseUnityPlugin bup && bup == plugin))
        {
            return;
        }

        var folderPath = Path.GetDirectoryName(plugin.Info.Location);
        var modInfoPath = Path.Combine(folderPath!, "swinfo.json");
        var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(plugin.Info.Location)!);
        if (File.Exists(modInfoPath))
        {
            if (!TryReadModInfo(plugin, modInfoPath, folderPath, out var metadata))
            {
                return;
            }

            var descriptor = new SpaceWarpPluginDescriptor(
                new BepInExModAdapter(plugin),
                metadata.Spec != SpecVersion.V1_2 ? metadata.ModID : plugin.Info.Metadata.GUID,
                metadata.Name,
                metadata,
                directoryInfo,
                false,
                new BepInExConfigFile(plugin.Config)
            );
            descriptor.Plugin!.SWMetadata = descriptor;
            if (!AssertSpecificationCompliance(descriptor, plugin, metadata))
            {
                return;
            }

            PluginList.RegisterPlugin(descriptor);
        }
        else
        {
            PluginList.RegisterPlugin(GetBepInExDescriptor(plugin));
        }
    }

    private static SpaceWarpPluginDescriptor GetBepInExDescriptor(BaseUnityPlugin plugin)
    {
        var pluginAdapter = new BepInExModAdapter(plugin);
        var descriptor = new SpaceWarpPluginDescriptor(
            pluginAdapter,
            plugin.Info.Metadata.GUID,
            plugin.Info.Metadata.Name,
            BepInExToSWInfo(plugin.Info),
            new DirectoryInfo(Path.GetDirectoryName(plugin.Info.Location)!),
            false,
            new BepInExConfigFile(plugin.Config)
        );
        pluginAdapter.SWMetadata = descriptor;

        return descriptor;
    }

    private static SpaceWarpPluginDescriptor GetBepInExDescriptor(PluginInfo info) => new(
        null,
        info.Metadata.GUID,
        info.Metadata.Name,
        BepInExToSWInfo(info),
        new DirectoryInfo(Path.GetDirectoryName(info.Location)!)
    );

    private static void RegisterAllBepInExMods()
    {
#pragma warning disable CS0618
        foreach (var plugin in Chainloader.Plugins)
#pragma warning restore CS0618
        {
            if (plugin is BaseSpaceWarpPlugin spaceWarpMod)
            {
                Logger.LogInfo($"Registering SpaceWarp plugin: {plugin.Info.Metadata.Name}");
                RegisterSingleSpaceWarpPlugin(spaceWarpMod);
            }
            else
            {
                Logger.LogInfo($"Registering BIE plugin: {plugin.Info.Metadata.Name}");
                RegisterSingleBepInExPlugin(plugin);
            }
        }
    }

    private static void RegisterAllCodelessMods()
    {
        var pluginPath = new DirectoryInfo(Paths.PluginPath);
        foreach (var swinfo in pluginPath.GetFiles("swinfo.json", SearchOption.AllDirectories))
        {
            ModInfo swinfoData;
            try
            {
                swinfoData = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(swinfo.FullName));
            }
            catch
            {
                Logger.LogError($"Error reading metadata file: {swinfo.FullName}, this mod will be ignored");
                continue;
            }

            if (swinfoData.Spec < SpecVersion.V1_3)
            {
                Logger.LogWarning(
                    $"Found swinfo information for: {swinfoData.Name}, but its spec is less than 1.3, if this " +
                    $"describes a \"codeless\" mod, it will be ignored"
                );
                continue;
            }

            var descriptor = new SpaceWarpPluginDescriptor(
                new AssetOnlyMod(swinfoData.Name),
                swinfoData.ModID,
                swinfoData.Name,
                swinfoData, swinfo.Directory,
                true,
                new BepInExConfigFile(FindOrCreateConfigFile(swinfoData.ModID))
            );
            descriptor.Plugin!.SWMetadata = descriptor;

            Logger.LogInfo($"Attempting to register codeless mod: {swinfoData.ModID}, {swinfoData.Name}");

            if (PluginList.AllPlugins.Any(
                    x => string.Equals(x.Guid, swinfoData.ModID, StringComparison.InvariantCultureIgnoreCase)
                ))
            {
                continue;
            }

            // Now we can just add it to our plugin list
            PluginList.RegisterPlugin(descriptor);
        }
    }

    private static ConfigFile FindOrCreateConfigFile(string guid)
    {
        var path = Path.Combine(Paths.ConfigPath, $"{guid}.cfg");
        return new ConfigFile(path, true);
    }

    private static ModInfo KspToSwinfo(Ksp2ModInfo mod)
    {
        var newInfo = new ModInfo
        {
            Spec = SpecVersion.V1_3,
            ModID = mod.ModName,
            Name = mod.ModName,
            Author = mod.ModAuthor,
            Description = mod.ModDescription,
            Source = "<unknown>",
            Version = mod.ModVersion.ToString(),
            Dependencies = new List<DependencyInfo>(),
            SupportedKsp2Versions = new SupportedVersionsInfo
            {
                Min = "*",
                Max = "*"
            },
            VersionCheck = null
        };
        return newInfo;
    }

    private static void RegisterSingleKspMod(DirectoryInfo folder)
    {
        ModInfo metadata;
        if (File.Exists(Path.Combine(folder.FullName, "swinfo.json")))
        {
            metadata = JsonConvert.DeserializeObject<ModInfo>(
                File.ReadAllText(Path.Combine(folder.FullName, "swinfo.json"))
            );
        }
        else if (File.Exists(Path.Combine(folder.FullName, "modinfo.json")))
        {
            var modinfo = JsonConvert.DeserializeObject<Ksp2ModInfo>(
                File.ReadAllText(Path.Combine(folder.FullName, "modinfo.json"))
            );
            metadata = KspToSwinfo(modinfo);
        }
        else
        {
            return;
        }

        // This descriptor *will* be modified later
        var descriptor = new SpaceWarpPluginDescriptor(
            null,
            metadata.ModID,
            metadata.Name,
            metadata,
            folder,
            false
        )
        {
            LatePreInitialize = true
        };
        PluginList.RegisterPlugin(descriptor);
    }

    private static void RegisterAllKspMods()
    {
        var pluginPath = new DirectoryInfo(Path.Combine(Paths.GameRootPath, "GameData", "Mods"));
        if (!pluginPath.Exists)
        {
            return;
        }

        Logger.LogInfo($"KSP Loaded mods path: {pluginPath.FullName}");
        // Let's quickly register every mod loaded with the internal loader into the load order before anything else,
        // with late pre-initialize
        foreach (var plugin in pluginPath.EnumerateDirectories())
        {
            Logger.LogInfo($"Attempting to register KSP loaded mod at {pluginPath.FullName}");
            RegisterSingleKspMod(plugin);
        }
    }

    private static void RegisterAllErroredMods()
    {
        // Lets do some magic here by copying some code to just get all the plugin types
        var allPlugins = TypeLoader.FindPluginTypes(
            Paths.PluginPath,
            Chainloader.ToPluginInfo,
            Chainloader.HasBepinPlugins,
            "chainloader"
        );
        foreach (var plugin in allPlugins)
        {
            foreach (var info in plugin.Value)
            {
                info.Location = plugin.Key;
                if (PluginList.AllPlugins.Any(
                        x => string.Equals(x.Guid, info.Metadata.GUID, StringComparison.InvariantCultureIgnoreCase) ||
                             (x.Plugin is BaseUnityPlugin baseUnityPlugin && string.Equals(
                                 baseUnityPlugin.Info.Metadata.GUID, info.Metadata.GUID,
                                 StringComparison.InvariantCultureIgnoreCase))
                    ))
                {
                    continue;
                }

                var descriptor = new SpaceWarpPluginDescriptor(
                    null,
                    info.Metadata.GUID,
                    info.Metadata.Name,
                    BepInExToSWInfo(info),
                    new DirectoryInfo(Path.GetDirectoryName(info.Location)!)
                );
                var errored = PluginList.GetErrorDescriptor(descriptor);
                errored.MissingDependencies = info.Dependencies.Select(x => x.DependencyGUID)
                    .Where(guid => PluginList.AllEnabledAndActivePlugins.All(x => x.Guid != guid))
                    .ToList();
            }
        }
    }

    private static void GetDisabledPlugins()
    {
        foreach (var plugin in ModList.DisabledPlugins)
        {
            GetSingleDisabledPlugin(plugin);
        }
    }

    private static void GetSingleDisabledPlugin(PluginInfo plugin)
    {
        var folderPath = Path.GetDirectoryName(plugin.Location);
        var swInfoPath = Path.Combine(folderPath!, "swinfo.json");
        if (Path.GetFileName(folderPath) != "plugins" && File.Exists(swInfoPath))
        {
            try
            {
                var swInfo = JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(swInfoPath));
                var descriptor = new SpaceWarpPluginDescriptor(
                    null,
                    plugin.Metadata.GUID,
                    plugin.Metadata.Name,
                    swInfo,
                    new DirectoryInfo(folderPath)
                );
                PluginList.RegisterPlugin(descriptor);
                PluginList.Disable(descriptor.Guid);
            }
            catch
            {
                var descriptor = GetBepInExDescriptor(plugin);
                PluginList.RegisterPlugin(descriptor);
                PluginList.Disable(descriptor.Guid);
            }
        }
        else
        {
            var descriptor = GetBepInExDescriptor(plugin);
            PluginList.RegisterPlugin(descriptor);
            PluginList.Disable(descriptor.Guid);
        }
    }

    private static void DisableMods()
    {
        foreach (var mod in ModList.DisabledPluginGuids)
        {
            PluginList.Disable(mod);
        }
    }
}