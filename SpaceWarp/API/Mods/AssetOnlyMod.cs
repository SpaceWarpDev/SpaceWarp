using BepInEx.Logging;
using SpaceWarp.API.Logging;

namespace SpaceWarp.API.Mods;

public class AssetOnlyMod : ISpaceWarpMod
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