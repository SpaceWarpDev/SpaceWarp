using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using SpaceWarp.Patches.Backend;
using SpaceWarp.Preload;
using SpaceWarp.Preload.API;

namespace SpaceWarp.Patches;

/// <summary>
/// Patches BepInEx's Chainloader.Start method to disable plugins, generate the mod paths DLL, compile Roslyn mods
/// and transform swinfo files to modinfo files.
/// </summary>
[HarmonyPatch]
internal static class ChainloaderPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Chainloader), nameof(Chainloader.Start))]
    private static void PreStartActions()
    {
        var trueLogger = Logger.CreateLogSource("Roslyn Compiler");

        // Compile Roslyn mods
        var changed = RoslynCompiler.CompileMods(trueLogger);

        // Generate the mod paths DLL
        PathsGenerator.GenerateSpaceWarpPathsDLL(changed, trueLogger);

        try
        {
            // Transform swinfo files in the internal mod loader folder to modinfo files
            ModInfoGenerator.TransformSwinfosToModInfos();
        }
        catch (Exception e)
        {
            trueLogger.LogError(e.ToString());
        }
    }

    private static bool CheckIfModIsDisabled(PluginInfo plugin, HashSet<string> deniedSet)
    {
        if (Array.IndexOf(ModList.DisabledPluginGuids, plugin.Metadata.GUID) == -1)
        {
            return true;
        }

        deniedSet.Add(plugin.Metadata.GUID);
        ModList.DisabledPlugins.Add(plugin);
        Entrypoint.LogSource.LogInfo($"{plugin.Metadata.GUID} was disabled, skipping loading...");
        return false;

    }
    [HarmonyILManipulator]
    [HarmonyPatch(typeof(Chainloader), nameof(Chainloader.Start))]
    private static void DisablePluginsIL(ILContext il)
    {
        ILCursor c = new(il);

        ILLabel continueLabel = default;
        c.GotoNext(
            MoveType.After,
            // this is from a continue, we use this to start the next iteration:
            x => x.MatchBrfalse(out continueLabel),
            x => x.MatchLdcI4(0), // false
            x => x.MatchStloc(24) // someBool = false
        );

        c.Emit(OpCodes.Ldloc, 23); // current PluginInfo
        c.Emit(OpCodes.Ldloc, 5); // set of denied plugins so far
        // false means skip to this plugin, true means continue loading it
        c.EmitDelegate(CheckIfModIsDisabled);
        c.Emit(OpCodes.Brfalse, continueLabel);
    }
}