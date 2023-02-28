using HarmonyLib;


namespace SpaceWarp.Patching;

[HarmonyPatch(typeof(KSP.Game.GameManager))]
[HarmonyPatch("StartGame")]
public class StartGamePatch
{
    public static void Prefix()
    {
        StartupManager.OnGameStarted();
    }
}