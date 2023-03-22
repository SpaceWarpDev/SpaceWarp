using HarmonyLib;
using KSP.Game;
using MonoMod.Cil;
using SpaceWarp.Patching.LoadingActions;

namespace SpaceWarp.Patching;

[HarmonyPatch]
internal static class BootstrapPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
    private static void GetSpaceWarpMods()
    {
        SpaceWarpManager.GetSpaceWarpPlugins();
    }

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.StartBootstrap))]
    private static void PatchInitializationsIL(ILContext ilContext, ILLabel endLabel)
    {
        ILCursor ilCursor = new(ilContext);

        var flowProp = AccessTools.DeclaredProperty(typeof(GameManager), nameof(GameManager.LoadingFlow));

        ilCursor.GotoNext(
            MoveType.After,
            instruction => instruction.MatchCallOrCallvirt(flowProp.SetMethod)
        );

        ilCursor.EmitDelegate(static () =>
        {
            foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins)
                GameManager.Instance.LoadingFlow.AddAction(new PreInitializeModAction(plugin));
        });

        ilCursor.GotoLabel(endLabel, MoveType.Before);
        ilCursor.Index -= 1;
        ilCursor.EmitDelegate(static () =>
        {
            var flow = GameManager.Instance.LoadingFlow;

            foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins)
            {
                flow.AddAction(new LoadAddressablesAction(plugin));
                flow.AddAction(new LoadLocalizationAction(plugin));
                flow.AddAction(new LoadAssetAction(plugin));
            }

            flow.AddAction(new LoadAddressablesLocalizationsAction());

            foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins) flow.AddAction(new InitializeModAction(plugin));

            foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins)
                flow.AddAction(new PostInitializeModAction(plugin));
        });
    }
}