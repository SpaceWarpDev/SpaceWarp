using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using SpaceWarpPatcher.API;
using SpaceWarpPatcher.Patches;

namespace SpaceWarpPatcher;

/// <summary>
/// Patcher for the UnityEngine.CoreModule assembly.
/// </summary>
[UsedImplicitly]
public static class Patcher
{
    internal static ManualLogSource LogSource;

    /// <summary>
    /// The target DLLs to patch.
    /// </summary>
    [UsedImplicitly]
    public static IEnumerable<string> TargetDLLs => ["UnityEngine.CoreModule.dll"];

    /// <summary>
    /// Patch the target assembly.
    /// </summary>
    /// <param name="asm">The target assembly.</param>
    [UsedImplicitly]
    public static void Patch(ref AssemblyDefinition asm)
    {
        LogSource = Logger.CreateLogSource("SpaceWarpPatcher");

        switch (asm.Name.Name)
        {
            case "UnityEngine.CoreModule":
                PatchCoreModule(asm);
                break;
        }
    }

    private static void PatchCoreModule(AssemblyDefinition asm)
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

[UsedImplicitly]
internal static class Delayer
{
    [UsedImplicitly]
    public static void PatchChainloaderStart()
    {
        ModList.Initialize();
        Harmony.CreateAndPatchAll(typeof(ChainloaderPatch));
    }
}