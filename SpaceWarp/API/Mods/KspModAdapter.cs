using KSP.Modding;
using SpaceWarp.API.Logging;

namespace SpaceWarp.API.Mods;

public class KspModAdapter : ISpaceWarpMod
{
    public KSP2Mod AdaptedMod;

    public KspModAdapter(KSP2Mod adaptedMod)
    {
        SWLogger = new UnityLogSource(adaptedMod.ModName);
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