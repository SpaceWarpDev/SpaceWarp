using BepInEx.Logging;
using SpaceWarp.API.Logging;
using SpaceWarp.API.Mods;

namespace SpaceWarp.Backend.Modding;

internal class AssetOnlyMod : ISpaceWarpMod
{
    public AssetOnlyMod(string name)
    {
        SWLogger = new BepInExLogger(new ManualLogSource(name));
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

    public ILogger SWLogger { get; }
}