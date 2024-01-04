using JetBrains.Annotations;
using KSP.Modding;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Logging;

namespace SpaceWarp.API.Mods;

/// <summary>
/// Base class for mods that are loaded by the KSP2 internal loader
/// </summary>
[PublicAPI]
public abstract class BaseKspLoaderSpaceWarpMod : Mod, ISpaceWarpMod
{
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

    /// <inheritdoc />
    public ILogger SWLogger { get; set; }

    /// <inheritdoc />
    public IConfigFile SWConfiguration { get; internal set; }

    /// <inheritdoc />
    public SpaceWarpPluginDescriptor SWMetadata { get; set; }
}