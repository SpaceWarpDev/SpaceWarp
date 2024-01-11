using HarmonyLib;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using SpaceWarp.Preload.API;

namespace SpaceWarp.Patches;

[UsedImplicitly]
internal class UnityCoreModulePatch : BasePatcher
{
    internal static UnityCoreModulePatch Instance { get; private set; }

    public UnityCoreModulePatch()
    {
        Instance = this;
    }

    public override IEnumerable<string> DLLsToPatch { get; } = ["UnityEngine.CoreModule.dll"];
    public override void ApplyPatch(ref AssemblyDefinition assembly)
    {
        // is this necessary? I (Windows10CE) didn't think so until i had to do it!
        var targetType = assembly.MainModule.GetType("UnityEngine.Application");
        var targetMethod = targetType.Methods.Single(x => x.Name == ".cctor");

        using var thisAsm = AssemblyDefinition.ReadAssembly(typeof(UnityCoreModulePatch).Assembly.Location);
        var delayer = thisAsm.MainModule.GetType("SpaceWarp.Patches.Delayer");
        var patchMethod = delayer.Methods.Single(m => m.Name == "PatchChainloaderStart");

        var il = new ILContext(targetMethod);
        var c = new ILCursor(il);
        c.GotoNext(
            MoveType.Before,
            x => x.MatchCall("BepInEx.Bootstrap.Chainloader", "Start")
        );

        c.Emit(OpCodes.Call, il.Module.ImportReference(patchMethod));
    }
}

internal static class Delayer
{
    [UsedImplicitly]
    private static void PatchChainloaderStart()
    {
        ModList.Initialize();
        Harmony.CreateAndPatchAll(typeof(ChainloaderPatch));
    }
}