using HarmonyLib;
using KSP.Game;
using MonoMod.Cil;
using SpaceWarp.API.Loading;
using SpaceWarp.Patching.LoadingActions;

namespace SpaceWarp.Patching;

[HarmonyPatch]
internal static class BootstrapPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
    private static void GetSpaceWarpMods()
    {
        SpaceWarpManager.GetAllPlugins();
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
            {
                if (plugin.Plugin != null)
                    GameManager.Instance.LoadingFlow.AddAction(new PreInitializeModAction(plugin.Plugin));
            }
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
                if (plugin.Plugin != null)
                {
                    foreach (var action in Loading.LoadingActionGenerators)
                    {
                        flow.AddAction(action(plugin.Plugin));
                    }
                }
                else
                {
                    foreach (var action in Loading.FallbackDescriptorLoadingActionGenerators)
                    {
                        flow.AddAction(action(plugin));
                    }
                }

                foreach (var action in Loading.DescriptorLoadingActionGenerators)
                {
                    flow.AddAction(action(plugin));
                }
            }

            flow.AddAction(new LoadAddressablesLocalizationsAction());
            foreach (var action in Loading.GeneralLoadingActions)
            {
                flow.AddAction(action);
            }
            
            foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins)
            {
                if (plugin.Plugin != null)
                    flow.AddAction(new InitializeModAction(plugin.Plugin));
            }
            
            foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins)
            {
                flow.AddAction(new LoadLuaAction(plugin));
            }
            

            foreach (var plugin in SpaceWarpManager.SpaceWarpPlugins)
            {
                if (plugin.Plugin != null)
                    flow.AddAction(new PostInitializeModAction(plugin.Plugin));
            }
        });
    }
}