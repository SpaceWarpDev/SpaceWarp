using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

[assembly: InternalsVisibleTo("SpaceWarp")]

namespace SpaceWarpPatcher;

public static class Patcher
{
    public static IEnumerable<string> TargetDLLs => new[] { "UnityEngine.CoreModule.dll" };

    public static void Patch(AssemblyDefinition asm)
    {
        // is this necessary? I (Windows10CE) didn't think so until i had to do it!
        var targetType = asm.MainModule.GetType("UnityEngine.Application");
        var targetMethod = targetType.Methods.Single(x => x.Name == ".cctor");

        using var thisAsm = AssemblyDefinition.ReadAssembly(typeof(Patcher).Assembly.Location);
        var delayer = thisAsm.MainModule.GetType("SpaceWarpPatcher.Delayer");
        var patchMethod = delayer.Methods.Single(m => m.Name == "PatchChainloaderStart");

        ILContext il = new(targetMethod);
        ILCursor c = new(il);
        c.GotoNext(MoveType.Before,
            x => x.MatchCall("BepInEx.Bootstrap.Chainloader", "Start")
        );

        c.Emit(OpCodes.Call, il.Module.ImportReference(patchMethod));
    }
}

internal static class Delayer
{
    public static void PatchChainloaderStart()
    {
        ChainloaderPatch.Logger = Logger.CreateLogSource("SW BIE Extensions");
        string disabledPluginsFilepath = Path.Combine(Paths.BepInExRootPath, "disabled_plugins.cfg");
        ChainloaderPatch.DisabledPluginsFilepath = disabledPluginsFilepath;
        if (!File.Exists(disabledPluginsFilepath))
        {
            File.Create(disabledPluginsFilepath).Dispose();
            ChainloaderPatch.Logger.LogWarning("Disabled plugins file did not exist, created empty file at: " + disabledPluginsFilepath);
        }
        ChainloaderPatch.DisabledPluginGuids = File.ReadAllLines(disabledPluginsFilepath);
        ChainloaderPatch.DisabledPlugins = new();
        Harmony.CreateAndPatchAll(typeof(ChainloaderPatch));
    }
}

[HarmonyPatch]
internal static class ChainloaderPatch
{
    internal static string[] DisabledPluginGuids;
    internal static string DisabledPluginsFilepath;
    internal static ManualLogSource Logger;
    internal static List<PluginInfo> DisabledPlugins;

    [HarmonyILManipulator]
    [HarmonyPatch(typeof(Chainloader), nameof(Chainloader.Start))]
    private static void DisablePluginsIL(ILContext il)
    {
        ILCursor c = new(il);

        ILLabel continueLabel = default;
        c.GotoNext(MoveType.After,
            x => x.MatchBrfalse(out continueLabel), // this is from a continue, we use this to start the next iteration
            x => x.MatchLdcI4(0), // false
            x => x.MatchStloc(24) // someBool = false
        );

        c.Emit(OpCodes.Ldloc, 23); // current PluginInfo
        c.Emit(OpCodes.Ldloc, 5); // set of denied plugins so far
        // false means skip to this plugin, true means continue loading it
        c.EmitDelegate(static bool(PluginInfo plugin, HashSet<string> deniedSet) =>
        {
            if (Array.IndexOf(DisabledPluginGuids, plugin.Metadata.GUID) != -1)
            {
                deniedSet.Add(plugin.Metadata.GUID);
                DisabledPlugins.Add(plugin);
                Logger.LogInfo($"{plugin.Metadata.GUID} was disabled, skipping loading...");
                return false;
            }
            return true;
        });
        c.Emit(OpCodes.Brfalse, continueLabel);
    }
}
