using System;
using HarmonyLib;
using KSP.Networking.MP.Utils;
using SpaceWarp.Backend.UI.Loading;
using UnityEngine;

namespace SpaceWarp.Patching;

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
        __instance._appStartLoadingScreenSpriteOptions.AddRange(LoadingScreenManager.LoadingScreens
            .Values);
    }
}