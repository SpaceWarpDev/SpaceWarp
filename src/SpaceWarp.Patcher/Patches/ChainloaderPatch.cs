using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using SpaceWarp.Patcher.API;
using SpaceWarp.Patcher.Backend;

namespace SpaceWarp.Patcher.Patches;

/// <summary>
/// Patches the Chainloader.Start method to disable plugins.
/// </summary>
[HarmonyPatch]
public static class ChainloaderPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Chainloader), nameof(Chainloader.Start))]
    private static void PreStartActions()
    {
        var trueLogger = Logger.CreateLogSource("Roslyn Compiler");
        var changed = RoslynCompiler.CompileMods(trueLogger);
        PathsGenerator.GenerateSpaceWarpPathsDLL(changed, trueLogger);
        try
        {
            SwinfoTransformer.TransformModSwinfos();
        }
        catch (Exception e)
        {
            trueLogger.LogError(e.ToString());
        }
    }

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(Chainloader), nameof(Chainloader.Start))]
    private static void DisablePluginsIL(ILContext il)
    {
        ILCursor c = new(il);

        ILLabel continueLabel = default;
        c.GotoNext(
            MoveType.After,
            x => x.MatchBrfalse(out continueLabel), // this is from a continue, we use this to start the next iteration
            x => x.MatchLdcI4(0), // false
            x => x.MatchStloc(24) // someBool = false
        );

        c.Emit(OpCodes.Ldloc, 23); // current PluginInfo
        c.Emit(OpCodes.Ldloc, 5); // set of denied plugins so far
        // false means skip to this plugin, true means continue loading it
        c.EmitDelegate(static bool (PluginInfo plugin, HashSet<string> deniedSet) =>
        {
            if (Array.IndexOf(ModList.DisabledPluginGuids, plugin.Metadata.GUID) == -1)
            {
                return true;
            }

            deniedSet.Add(plugin.Metadata.GUID);
            ModList.DisabledPlugins.Add(plugin);
            Patcher.LogSource.LogInfo($"{plugin.Metadata.GUID} was disabled, skipping loading...");
            return false;

        });
        c.Emit(OpCodes.Brfalse, continueLabel);
    }
}