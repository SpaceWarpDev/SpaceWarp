using KSP.Game;
using SpaceWarp.API;
using SpaceWarp.API.Logging;
using SpaceWarp.API.Managers;
using SpaceWarp.Patching.LoadingActions;


namespace SpaceWarp.Patching;

/// <summary>
/// Patches the loading screen to add the mod loading
/// </summary>
public class LoadingScreenPatcher
{
    public static void AddModLoadingScreens()
    {
        GameManager gameManager = GameManager.Instance;
        gameManager.LoadingFlow.AddAction(new ReadingModsAction("Resolving Space Warp Mod Load Order"));
        gameManager.LoadingFlow.AddAction(new SpaceWarpAssetInitializationAction("Initializing Space Warp Provided Assets"));
    }

    public static void AddAllModLoadingSteps()
    {
        GameManager gameManager = GameManager.Instance;
        ModLogger logger = new ModLogger(gameManager.name);

        if (!ManagerLocator.TryGet(out SpaceWarpManager spaceWarpManager))
        {
            logger.Error("Space Warp Manager not found. Cannot add mod loading steps.");
            return;
        }
        
        foreach (var mod in spaceWarpManager._modLoadOrder)
        {
            gameManager.LoadingFlow.AddAction(new LoadAssetAction($"Loading assets for {mod.Item1}",mod.Item1, mod.Item2));
            gameManager.LoadingFlow.AddAction(new LoadModAction($"Initializing {mod.Item1}",mod.Item1, mod.Item2));
        }
            
        gameManager.LoadingFlow.AddAction(new AfterModsLoadedAction("Space Warp Mod Post-Initialization"));
    }
}