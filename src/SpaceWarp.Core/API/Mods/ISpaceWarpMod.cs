using JetBrains.Annotations;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Logging;

namespace SpaceWarp.API.Mods;

/// <summary>
/// Interface for all SpaceWarp mods
/// </summary>
[PublicAPI]
public interface ISpaceWarpMod
{
    /// <summary>
    /// <para>1st stage initialization.</para>
    /// <para>This is called before any of the game is actually loaded, it is called as early as possible in the game's
    /// bootstrap process.</para>
    /// </summary>
    public void OnPreInitialized();

    /// <summary>
    /// <para>2nd stage initialization.</para>
    /// <para>This is called after the game is loaded, and after your mods assets are loaded.</para>
    /// </summary>
    public void OnInitialized();

    /// <summary>
    /// <para>3rd stage initialization.</para>
    /// <para>This is called after all mods have done 2nd stage initialization.</para>
    /// </summary>
    public void OnPostInitialized();

    /// <summary>
    /// Gets the logger for this mod
    /// </summary>
    public ILogger SWLogger { get; }

    /// <summary>
    /// Gets the configuration file for this mod
    /// </summary>
    public IConfigFile SWConfiguration { get; }

    /// <summary>
    /// Gets the metadata for this mod
    /// </summary>
    public SpaceWarpPluginDescriptor SWMetadata { get; set; }
}