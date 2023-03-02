using HarmonyLib;
using KSP.Game;
using KSP.Game.StartupFlow;
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
    private static void PatchInitializationsIL(ILContext il, ILLabel endLabel)
    {
        ILCursor c = new(il);

        var flowProp = AccessTools.DeclaredProperty(typeof(GameManager), nameof(GameManager.LoadingFlow));

        c.GotoNext(MoveType.After,
            x => x.MatchCallOrCallvirt(flowProp.SetMethod)
        );

        c.EmitDelegate(static () =>
        {
            foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins)
            {
                GameManager.Instance.LoadingFlow.AddAction(new PreInitializeModAction(plugin));
            }
        });

        c.GotoNext(MoveType.Before,
            x => x.MatchLdarg(0),
            x => x.MatchCallOrCallvirt(flowProp.GetMethod),
            x => x.MatchLdloc(out _),
            x => x.MatchNewobj(out var ctor) && ctor.DeclaringType.FullName == typeof(CreateMainMenuFlowAction).FullName
        );

        c.EmitDelegate(static () =>
        {
            foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins)
            {
                GameManager.Instance.LoadingFlow.AddAction(new InitializeModAction(plugin));
            }
        });

        c.GotoLabel(endLabel);
        c.EmitDelegate(static () =>
        {
            foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins)
            {
                GameManager.Instance.LoadingFlow.AddAction(new PostInitializeModAction(plugin));
            }
        });
    }
}
