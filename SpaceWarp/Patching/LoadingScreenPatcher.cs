using HarmonyLib;
using KSP.Game;
using SpaceWarp.API;
using SpaceWarp.API.Managers;
using SpaceWarp.Patching.LoadingActions;


namespace SpaceWarp.Patching
{
    /// <summary>
    /// Patches the loading screen to add the mod loading
    /// </summary>
    public class LoadingScreenPatcher
    {
        public static void AddModLoadingScreens()
        {	
            GameManager gameManager = GameManager.Instance;

            gameManager.LoadingFlow.AddAction(new ReadingModsAction("Resolving Space Warp Mod Load Order"));
            gameManager.LoadingFlow.AddAction(new LoadAssetsAction("Loading Space Warp Assets"));
			gameManager.LoadingFlow.AddAction(new LoadModsAction("Loading Space Warp Mods"));
            gameManager.LoadingFlow.AddAction(new AfterModsLoadedAction("Space Warp Mod Post-Initialization"));
        }
    }
}