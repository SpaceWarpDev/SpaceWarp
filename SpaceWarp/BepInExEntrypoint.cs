#if !DOORSTOP_BUILD
using BepInEx;

namespace SpaceWarp
{
    [BepInPlugin("com.github.x606.spacewarp", "SpaceWarp", MyPluginInfo.PLUGIN_VERSION)]
    public class BepInExEntrypoint : BaseUnityPlugin
    {
        public void Awake()
        {
            SpaceWarpEntrypoint.Start();
        }
    }
}
#endif
