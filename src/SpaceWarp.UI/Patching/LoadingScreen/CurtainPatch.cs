using HarmonyLib;
using SpaceWarp.Backend.UI.Loading;

namespace SpaceWarp.Patching.LoadingScreen;

[HarmonyPatch(typeof(Curtain))]
internal static class CurtainPatch
{
    internal static LoadingScreenManager LoadingScreenManager;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Curtain.Awake))]
    public static void LoadScreensEarly(Curtain __instance)
    {
        LoadingScreenManager = new LoadingScreenManager();
        LoadingScreenManager.LoadScreens(__instance);
        __instance._appStartLoadingScreenSpriteOptions.AddRange(
            LoadingScreenManager.LoadingScreens.Values
        );
    }
}