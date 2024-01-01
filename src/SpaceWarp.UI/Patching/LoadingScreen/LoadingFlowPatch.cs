using HarmonyLib;
using KSP.Game;
using KSP.Game.Flow;
using KSP.Startup;

namespace SpaceWarp.Patching.LoadingScreen;

[HarmonyPatch(typeof(SequentialFlow))]
internal static class LoadingFlowPatch
{
    private static long _loadingScreenTimer;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SequentialFlow.NextFlowAction))]
    internal static void SelectRandomLoadingScreen(SequentialFlow __instance)
    {
        if (__instance.FlowState != FlowState.Finished)
        {
            var time = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
            if (time - _loadingScreenTimer >= 7)
            {
                _loadingScreenTimer = time;
                // Switch loading screen
                if (GameManager.Instance == null || GameManager.Instance.Game == null ||
                    GameManager.Instance.Game.UI == null) return;

                var curtain = GameManager.Instance.Game.UI.Curtain;
                if (curtain == null) return;
                // Just to update this as well
                curtain._curDefaultLoadingScreenSprite = curtain.PickRandomStartingSprite();
                var ctx = curtain._curtainContextData;
                curtain.SelectRandomImage(ctx);
                curtain.SetImage(ctx);
            }
        }
    }
}