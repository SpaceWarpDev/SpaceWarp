using System;
using HarmonyLib;
using KSP.Game;
using KSP.Game.Flow;
using KSP.Startup;
using UnityEngine;

namespace SpaceWarp.Patching;

[HarmonyPatch(typeof(SequentialFlow))]
internal static class LoadingFlowPatch
{
    internal static long LoadingScreenTimer;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SequentialFlow.NextFlowAction))]
    internal static void SelectRandomLoadingScreen(SequentialFlow __instance)
    {
        if (__instance.FlowState != FlowState.Finished)
        {
            var time = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
            if (time - LoadingScreenTimer >= 7)
            {
                LoadingScreenTimer = time;
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