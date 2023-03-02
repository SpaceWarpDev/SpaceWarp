global using UnityObject = UnityEngine.Object;
global using System.Linq;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SpaceWarp.UI;

namespace SpaceWarp;

[BepInPlugin(ModGuid, ModName, MyPluginInfo.PLUGIN_VERSION)]
public sealed class SpaceWarpPlugin : BaseUnityPlugin
{
    public const string ModGuid = "com.github.x606.spacewarp";
    public const string ModName = "Space Warp";

    new internal ManualLogSource Logger => base.Logger;
        
    public void Awake()
    {
        BepInEx.Logging.Logger.Listeners.Add(new SpaceWarpConsoleLogListener());
            
        Harmony.CreateAndPatchAll(typeof(SpaceWarpPlugin).Assembly, ModGuid);
        
        SpaceWarpManager.Initialize(this);
    }
}
