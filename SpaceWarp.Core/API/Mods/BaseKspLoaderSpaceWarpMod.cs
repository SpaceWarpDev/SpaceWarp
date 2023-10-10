using JetBrains.Annotations;
using KSP.Modding;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Logging;

namespace SpaceWarp.API.Mods;

[PublicAPI]
public abstract class BaseKspLoaderSpaceWarpMod : Mod, ISpaceWarpMod
{
    public virtual void OnPreInitialized()
    {

    }

    public virtual void OnInitialized()
    {
    }

    public virtual void OnPostInitialized()
    {
    }

    /// <summary>
    /// Gets set automatically, before awake is called
    /// </summary>
    public ILogger SWLogger { get; set; }

    public IConfigFile SWConfiguration {
        get;
        internal set;
    }

    public SpaceWarpPluginDescriptor SWMetadata { get; set; }
}