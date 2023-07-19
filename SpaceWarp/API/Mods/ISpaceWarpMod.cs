using SpaceWarp.API.Logging;

namespace SpaceWarp.API.Mods;

public interface ISpaceWarpMod
{
    public void OnPreInitialized();

    public void OnInitialized();

    public void OnPostInitialized();

    public ILogger SWLogger { get; }
}