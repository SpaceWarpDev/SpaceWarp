using JetBrains.Annotations;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Logging;

namespace SpaceWarp.API.Mods;

[PublicAPI]
public interface ISpaceWarpMod
{
    public void OnPreInitialized();

    public void OnInitialized();

    public void OnPostInitialized();

    public ILogger SWLogger { get; }

    public IConfigFile SWConfiguration { get; }

    public SaveConfigFile CampaignConfiguration { get; }

    public SpaceWarpPluginDescriptor SWMetadata { get; set; }
}