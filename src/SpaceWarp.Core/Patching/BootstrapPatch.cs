using HarmonyLib;
using KSP.Game;
using KSP.Game.Flow;
using MonoMod.Cil;
using SpaceWarp.API.Loading;
using SpaceWarp.API.Mods;
using SpaceWarp.Backend.Modding;
using SpaceWarp.Patching.LoadingActions;

namespace SpaceWarp.Patching;

[HarmonyPatch]
internal static class BootstrapPatch
{
    internal static bool ForceSpaceWarpLoadDueToError = false;
    internal static SpaceWarpPluginDescriptor ErroredSWPluginDescriptor;

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
    [HarmonyPrefix]
    private static void GetAllMods()
    {
        PluginRegister.RegisterAllMods();
        PluginList.ResolveDependenciesAndLoadOrder();
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

        ilCursor.EmitDelegate(InjectBeforeGameLoadMethods);

        ilCursor.GotoLabel(endLabel, MoveType.Before);
        ilCursor.Index -= 1;
        ilCursor.EmitDelegate(InjectAfterGameLoadMethods);
    }

    private static void InjectAfterGameLoadMethods()
    {
        var flow = GameManager.Instance.LoadingFlow;
        var allPlugins = GetAllPlugins();

        LatePreinitialize(allPlugins);
        DoLoadingActions(allPlugins, flow);
        foreach (var actionGenerator in Loading.GeneralLoadingActions)
        {
            flow.AddAction(actionGenerator());
        }

        foreach (var plugin in allPlugins)
        {
            flow.AddAction(new InitializeModAction(plugin));
        }

        foreach (var plugin in allPlugins)
        {
            flow.AddAction(new LoadLuaAction(plugin));
        }

        foreach (var plugin in allPlugins)
        {
            flow.AddAction(new PostInitializeModAction(plugin));
        }
    }

    private static void DoLoadingActions(IList<SpaceWarpPluginDescriptor> allPlugins, SequentialFlow flow)
    {
        foreach (var plugin in allPlugins)
        {
            flow.AddAction(new LoadAddressablesAction(plugin));
            flow.AddAction(new LoadLocalizationAction(plugin));
            DoOldStyleLoadingActions(flow, plugin);

            foreach (var action in Loading.DescriptorLoadingActionGenerators)
            {
                flow.AddAction(action(plugin));
            }
        }
    }

    private static void DoOldStyleLoadingActions(SequentialFlow flow, SpaceWarpPluginDescriptor plugin)
    {
        if (plugin.Plugin is not BaseSpaceWarpPlugin baseSpaceWarpPlugin) return;
        foreach (var action in Loading.LoadingActionGenerators)
        {
            flow.AddAction(action(baseSpaceWarpPlugin));
        }
    }

    private static void LatePreinitialize(IList<SpaceWarpPluginDescriptor> allPlugins)
    {
        foreach (var plugin in allPlugins.Where(plugin => plugin.LatePreInitialize))
        {
            GameManager.Instance.LoadingFlow.AddAction(new PreInitializeModAction(plugin));
        }
    }

    private static IList<SpaceWarpPluginDescriptor> GetAllPlugins()
    {
        IList<SpaceWarpPluginDescriptor> allPlugins;
        if (ForceSpaceWarpLoadDueToError)
        {
            var l = new List<SpaceWarpPluginDescriptor> { ErroredSWPluginDescriptor };
            l.AddRange(PluginList.AllEnabledAndActivePlugins);
            allPlugins = l;
        }
        else
        {
            allPlugins = PluginList.AllPlugins.ToList();
        }

        return allPlugins;
    }

    private static void InjectBeforeGameLoadMethods()
    {
        if (ForceSpaceWarpLoadDueToError)
        {
            GameManager.Instance.LoadingFlow.AddAction(new PreInitializeModAction(ErroredSWPluginDescriptor));
        }

        foreach (var plugin in PluginList.AllEnabledAndActivePlugins)
        {
            if (plugin.Plugin != null && !plugin.LatePreInitialize)
                GameManager.Instance.LoadingFlow.AddAction(new PreInitializeModAction(plugin));
        }
    }
}