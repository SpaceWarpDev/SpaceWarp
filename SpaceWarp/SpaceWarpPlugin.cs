global using UnityObject = UnityEngine.Object;
global using System.Linq;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SpaceWarp.UI;

namespace SpaceWarp;

[BepInPlugin(ModGuid, ModName, ModVer)]
public sealed class SpaceWarpPlugin : BaseUnityPlugin
{
    public const string ModGuid = "com.github.x606.spacewarp";
    public const string ModName = "Space Warp";
    public const string ModVer = MyPluginInfo.PLUGIN_VERSION;

    new internal ManualLogSource Logger => base.Logger;
        
    public void Awake()
    {
        BepInEx.Logging.Logger.Listeners.Add(new SpaceWarpConsoleLogListener());
            
        Harmony.CreateAndPatchAll(typeof(SpaceWarpPlugin).Assembly, ModGuid);
        
        SpaceWarpManager.Initialize(this);
    }
}
