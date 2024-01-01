using BepInEx;
using JetBrains.Annotations;
using KSP.Game;
using KSP.Messages;
using KSP.VFX;
using SpaceWarp.API.Mods.JSON;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Logging;

namespace SpaceWarp.API.Mods;

/// <summary>
/// Represents a SpaceWarp mod based on BepInEx.
/// </summary>
[PublicAPI]
public abstract class BaseSpaceWarpPlugin : BaseUnityPlugin, ISpaceWarpMod
{
    #region KerbalMonoBehaviour properties

    /// <summary>
    /// The current game instance, this is null if the game is not yet initialized or shutting down.
    /// </summary>
    protected static GameInstance Game => GameManager.Instance == null ? null : GameManager.Instance.Game;

    /// <summary>
    /// The message center for the current game instance.
    /// </summary>
    protected MessageCenter Messages => Game.Messages;

    /// <summary>
    /// The current game instance, this is null if the game is not yet initialized or shutting down.
    /// </summary>
    // ReSharper disable Unity.NoNullPropagation - fine because it's null checked by Game properly
    // ReSharper disable once InconsistentNaming
    protected ContextualFxSystem CFXSystem => Game?.GraphicsManager?.ContextualFxSystem;

    /// <summary>
    /// Whether the game is shutting down.
    /// </summary>
    protected bool IsGameShuttingDown => Game == null;

    #endregion

    private BepInExLogger _logger;
    private BepInExConfigFile _configFile;

    /// <summary>
    /// The mod info for this mod.
    /// </summary>
    public ModInfo SpaceWarpMetadata { get; internal set; }

    /// <summary>
    /// The path to the folder containing the plugin.
    /// </summary>
    public string PluginFolderPath { get; internal set; }

    /// <summary>
    /// The correct ID of the mod based on its spec version.
    /// </summary>
    public string IdBySpec => GetGuidBySpec(Info, SpaceWarpMetadata);

    /// <inheritdoc />
    public ILogger SWLogger => _logger ??= new BepInExLogger(Logger);

    /// <inheritdoc />
    public IConfigFile SWConfiguration => _configFile ??= new BepInExConfigFile(Config);

    /// <inheritdoc />
    public SpaceWarpPluginDescriptor SWMetadata { get; set; }

    /// <inheritdoc />
    public virtual void OnPreInitialized()
    {
    }

    /// <inheritdoc />
    public virtual void OnInitialized()
    {
    }

    /// <inheritdoc />
    public virtual void OnPostInitialized()
    {
    }

    internal static string GetGuidBySpec(PluginInfo pluginInfo, ModInfo modInfo)
    {
        return modInfo.Spec >= SpecVersion.V1_2
            ? pluginInfo.Metadata.GUID
            : modInfo.ModID;
    }
}