using System;
using System.Collections.Generic;
using HarmonyLib;
using Mono.Cecil;
// ReSharper disable UnusedMember.Global

namespace ChainloaderPatcher;

public static class Patcher
{
    public static IEnumerable<string> TargetDLLs
    {
        get
        {
            Harmony.CreateAndPatchAll(typeof(Patcher).Assembly, typeof(Patcher).Namespace);
            return Array.Empty<string>();
        }
    }

    // ReSharper disable once UnusedParameter.Global
    public static void Patch(AssemblyDefinition assembly)
    {
    }
}