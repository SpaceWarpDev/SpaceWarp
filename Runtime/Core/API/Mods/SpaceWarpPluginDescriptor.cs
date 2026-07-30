using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using ReduxLib.Configuration;
using SpaceWarp2.API.Mods.JSON;
using UnityEngine.AddressableAssets.ResourceLocators;
using ILogger = ReduxLib.Logging.ILogger;

namespace SpaceWarp2.API.Mods;

/// <summary>
/// A descriptor for a SpaceWarp plugin.
/// </summary>
[PublicAPI]
public class SpaceWarpPluginDescriptor
{
    /// <summary>
    /// Creates a new plugin descriptor.
    /// </summary>
    /// <param name="plugin">The plugin instance.</param>
    /// <param name="guid">The plugin's GUID.</param>
    /// <param name="name">The plugin's name.</param>
    /// <param name="swInfo">The plugin's swinfo.</param>
    /// <param name="folder">The plugin's folder.</param>
    /// <param name="doLoadingActions">Whether or not to do loading actions.</param>
    /// <param name="configFile">The plugin's config file.</param>
    public SpaceWarpPluginDescriptor(
        ISpaceWarpMod? plugin,
        string guid,
        string name,
        ModInfo swInfo,
        DirectoryInfo folder,
        bool doLoadingActions = true,
        IConfigFile? configFile = null
    )
    {
        Plugin = plugin;
        Guid = guid;
        Name = name;
        SWInfo = swInfo;
        Folder = folder;
        DoLoadingActions = doLoadingActions;
        ConfigFile = configFile;
        AddressableScriptLabel = swInfo.AddressableScriptLabel;
        AddressablePrefabPatchLabel =
            swInfo.AddressablePrefabPatchLabel;
    }

    /// <summary>
    /// The plugin instance.
    /// </summary>
    public ISpaceWarpMod? Plugin;

    private ILogger _logger;

    /// <summary>
    /// Gets or sets this mod's logger, resolved lazily from its <see cref="Guid" />.
    /// </summary>
    /// <remarks>
    /// Always present, unlike <see cref="Plugin" /> (an asset-only mod has no plugin), so loading code logs
    /// through the descriptor rather than the optional plugin. The plugin's own <c>SWLogger</c> is injected
    /// from this when the plugin is created.
    /// </remarks>
    public ILogger Logger
    {
        get => _logger ??= ReduxLib.ReduxLib.GetLogger(Guid);
        set => _logger = value;
    }

    /// <summary>
    /// Every assembly that belongs to this mod - its main assembly plus any libraries loaded from its lib folder.
    /// Used to attribute code, such as PatchManager C# patches, back to this mod.
    /// </summary>
    public readonly List<Assembly> Assemblies = new();

    /// <summary>
    /// Addressables catalogs loaded from this mod's distribution folder.
    /// Keeps catalog provenance associated with the swinfo descriptor so
    /// consumers can attribute labeled assets without duplicating mod
    /// metadata inside those assets.
    /// </summary>
    public readonly List<IResourceLocator> AddressableResourceLocators = new();

    /// <summary>
    /// Lua lifecycle closures registered by this mod's scripts via the SW global, fired during the Init phase
    /// before the C# OnInitialized.
    /// </summary>
    /// <remarks>
    /// Populated game-side. Plain Actions so the descriptor never needs to name a game-side script type.
    /// </remarks>
    public readonly List<Action> InitHooks = new();

    /// <summary>
    /// Lua lifecycle closures fired during the PostInit phase, before the C# OnPostInitialized.
    /// </summary>
    public readonly List<Action> PostInitHooks = new();

    /// <summary>
    /// Lua script files belonging to this mod that the runtime runs, set at discovery.
    /// </summary>
    public readonly List<string> ScriptFiles = new();

    /// <summary>
    /// The script sources the runtime ran for this mod, keyed by source identity with the script text as the
    /// value.
    /// </summary>
    /// <remarks>
    /// Read by PatchManager to feed its patch-cache checksum without re-reading the files.
    /// </remarks>
    public readonly Dictionary<string, string> LoadedScripts = new();

    /// <summary>
    /// Errors thrown by this mod's scripts while the runtime ran them, keyed by source identity.
    /// </summary>
    /// <remarks>
    /// Read by PatchManager into its patch summary.
    /// </remarks>
    public readonly List<(string key, string reason)> ScriptErrors = new();

    /// <summary>
    /// An addressables label whose text assets are this mod's scripts, for mods that ship their scripts as
    /// addressables rather than loose files. Null for file-based mods.
    /// </summary>
    public string? AddressableScriptLabel;

    /// <summary>
    /// An Addressables label whose text assets are this mod's declarative
    /// prefab-patch manifests. Ownership comes from this descriptor rather
    /// than from the manifest asset's Addressables address.
    /// </summary>
    public string? AddressablePrefabPatchLabel;

    /// <summary>
    /// The plugin's GUID.
    /// </summary>
    public readonly string Guid;

    /// <summary>
    /// The plugin's name.
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// The plugin's swinfo.
    /// </summary>
    public readonly ModInfo SWInfo;

    /// <summary>
    /// The plugin's folder.
    /// </summary>
    public readonly DirectoryInfo Folder;

    /// <summary>
    /// Whether or not to do loading actions.
    /// </summary>
    public bool DoLoadingActions;

    /// <summary>
    /// The plugin's config file.
    /// </summary>
    public IConfigFile? ConfigFile;

    /// <summary>
    /// Whether or not the plugin is outdated. Set by the version checking system.
    /// </summary>
    public bool Outdated;

    /// <summary>
    /// Whether or not the plugin is unsupported.
    /// </summary>
    public bool Unsupported;

    /// <summary>
    /// Whether or not the plugin has been pre-initialized yet.
    /// </summary>
    public bool LatePreInitialize;

    /// <summary>
    /// Whether this is a core mod (controls which part of the mod list it goes into).
    /// </summary>
    public bool IsCore;

    /// <summary>
    /// Is this a KSP1 mod imported via the KSP1 mod importer (controls which foldout in the mod list it goes into).
    /// </summary>
    public bool IsKsp1;
}
