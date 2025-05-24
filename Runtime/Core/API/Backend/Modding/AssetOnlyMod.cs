using ReduxLib.Configuration;
using ReduxLib.Logging;
using SpaceWarp.API.Mods;

namespace SpaceWarp.API.Backend.Modding;

internal class AssetOnlyMod : ISpaceWarpMod
{
    public AssetOnlyMod(string name)
    {
        SWLogger = ReduxLib.ReduxLib.GetLogger(name);
    }

    public void OnPreInitialized()
    {
    }

    public void OnInitialized()
    {
    }

    public void OnPostInitialized()
    {
    }

    public ILogger SWLogger { get; set; }
    public IConfigFile SWConfiguration { get; set; } = new EmptyConfigFile();

    public SpaceWarpPluginDescriptor SWMetadata { get; set; }
}