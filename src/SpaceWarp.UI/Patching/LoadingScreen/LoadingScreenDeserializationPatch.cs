using HarmonyLib;
using KSP.Game;
using KSP.Game.StartupFlow;

namespace SpaceWarp.Patching.LoadingScreen;

[HarmonyPatch(typeof(DeserializeLoadingScreens))]
internal static class LoadingScreenDeserializationPatch
{
    [HarmonyPatch(nameof(DeserializeLoadingScreens.DoAction))]
    [HarmonyPostfix]
    private static void AddToAllFlows()
    {
        var curtain = GameManager.Instance.Game.UI.Curtain;
        foreach (var flow in curtain.LoadingScreens.Values.Distinct())
        {
            flow.ScreenKeys.AddRange(CurtainPatch.LoadingScreenManager.LoadingScreens.Keys);
        }
    }
}