using KSP.Modding;
using SpaceWarp.API.Configuration;
using SpaceWarp.API.Logging;
using SpaceWarp.API.Mods;
using UnityEngine;
using ILogger = SpaceWarp.API.Logging.ILogger;

namespace SpaceWarp.Backend.Modding;

internal class KspModAdapter : MonoBehaviour, ISpaceWarpMod
{
    public KSP2Mod AdaptedMod;

    public void OnPreInitialized()
    {
        AdaptedMod.modCore?.ModStart();
        SWLogger = new UnityLogSource(AdaptedMod.ModName);
    }

    public void OnInitialized()
    {
    }

    public void OnPostInitialized()
    {
    }

    public void Update()
    {
        AdaptedMod.modCore?.ModUpdate();
    }

    public ILogger SWLogger { get; private set; }
    public IConfigFile SWConfiguration => new EmptyConfigFile();
    public SpaceWarpPluginDescriptor SWMetadata { get; set; }
    
    private SaveConfigFile _saveConfig;
    public SaveConfigFile CampaignConfiguration => _saveConfig ??= new SaveConfigFile();
}