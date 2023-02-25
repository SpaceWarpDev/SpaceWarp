using HarmonyLib;
using SpaceWarp.API;
using SpaceWarp.Patching.LoadingActions;


namespace SpaceWarp.Patching
{
    // [HarmonyPatch(typeof(KSP.Game.GameManager),nameof(KSP.Game.GameManager.StartBootstrap))]
    // public class LoadingScreenPatcher
    // {
    //     public static SpaceWarpManager Manager;
    //     static void Postfix(KSP.Game.GameManager __instance)
    //     {
    //         __instance.LoadingFlow.AddAction(new LoadModsAction("Loading Space Warp Mods",Manager));
    //         __instance.LoadingFlow.AddAction(new AfterModsLoadedAction("Doing Mod Finalization",Manager));
    //     }
    // }
    public class LoadingScreenPatcher
    {
        public static void AddScreens(KSP.Game.GameManager gameManager, SpaceWarpManager spaceWarpManager)
        {
            gameManager.LoadingFlow.AddAction(new LoadModsAction("Loading Space Warp Mods",spaceWarpManager));
            gameManager.LoadingFlow.AddAction(new AfterModsLoadedAction("Loading Space Warp Mods",spaceWarpManager));
        }
    }
}