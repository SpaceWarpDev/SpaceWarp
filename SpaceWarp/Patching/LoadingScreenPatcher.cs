using KSP.Game;
using SpaceWarp.API;
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
        gameManager.LoadingFlow.AddAction(
            new LoadSpaceWarpAddressablesAction("Initializing Space Warp Provided Addressables"));
        gameManager.LoadingFlow.AddAction(
            new SpaceWarpAssetInitializationAction("Initializing Space Warp Provided Assets"));
        gameManager.LoadingFlow.AddAction(
            new LoadSpaceWarpLocalizationsAction("Initializing Space Warp Localizations"));
    }

    public static void AddAllModLoadingSteps()
    {
        GameManager gameManager = GameManager.Instance;
        if (!ManagerLocator.TryGet(out SpaceWarpManager spaceWarpManager)) return; //TODO: Log a message here
        foreach (var mod in spaceWarpManager._modLoadOrder)
        {
            gameManager.LoadingFlow.AddAction(new LoadAddressablesAction($"Loading addressables for {mod.Item1}",
                mod.Item1, mod.Item2));
            gameManager.LoadingFlow.AddAction(new LoadAssetAction($"Loading assets for {mod.Item1}", mod.Item1,
                mod.Item2));
            gameManager.LoadingFlow.AddAction(new LoadLocalizationAction($"Loading localizations for {mod.Item1}", mod.Item1, mod.Item2));
            gameManager.LoadingFlow.AddAction(new LoadModAction($"Initializing {mod.Item1}", mod.Item1, mod.Item2));
        }

        gameManager.LoadingFlow.AddAction(new AfterModsLoadedAction("Space Warp Mod Post-Initialization"));
    }
}