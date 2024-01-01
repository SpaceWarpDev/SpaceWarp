using BepInEx;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Logging;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Backend.Modding;

internal class BepInExModAdapter : ISpaceWarpMod
{
    public readonly BaseUnityPlugin Plugin;

    /// <inheritdoc />
    public void OnPreInitialized()
    {
    }

    /// <inheritdoc />
    public void OnInitialized()
    {
    }

    public void OnPostInitialized()
    {
    }

    public ILogger SWLogger => new BepInExLogger(Plugin.Logger);
    public IConfigFile SWConfiguration => new BepInExConfigFile(Plugin.Config);
    public SpaceWarpPluginDescriptor SWMetadata { get; set; }

    public BepInExModAdapter(BaseUnityPlugin plugin)
    {
        Plugin = plugin;
    }
}