global using UnityObject = UnityEngine.Object;
global using System.Linq;

using BepInEx;
using HarmonyLib;
using SpaceWarp.UI;

namespace SpaceWarp;

[BepInPlugin(ModGuid, ModName, MyPluginInfo.PLUGIN_VERSION)]
public sealed class SpaceWarpPlugin : BaseUnityPlugin
{
    public const string ModGuid = "com.github.x606.spacewarp";
    public const string ModName = "Space Warp";
        
    public void Awake()
    {
        BepInEx.Logging.Logger.Listeners.Add(new SpaceWarpConsoleLogListener());
            
        Harmony.CreateAndPatchAll(typeof(SpaceWarpPlugin).Assembly, ModGuid);
    }
}
