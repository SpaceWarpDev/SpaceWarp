using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SpaceWarp.Patching;

internal static class CodeGenerationUtilities
{
    /// <summary>
    ///     Creates a CodeInstruction that pushed an integer to the stack.
    /// </summary>
    /// <param name="i">The integer to push</param>
    /// <returns>An new CodeInstruction to push i</returns>
    internal static CodeInstruction PushIntInstruction(int i)
    {
        switch (i) {
            case 0: return new(OpCodes.Ldc_I4_0);
            case 1: return new(OpCodes.Ldc_I4_1);
            case 2: return new(OpCodes.Ldc_I4_2);
            case 3: return new(OpCodes.Ldc_I4_3);
            case 4: return new(OpCodes.Ldc_I4_4);
            case 5: return new(OpCodes.Ldc_I4_5);
            case 6: return new(OpCodes.Ldc_I4_6);
            case 7: return new(OpCodes.Ldc_I4_7);
            case 8: return new(OpCodes.Ldc_I4_8);
            default: return new(OpCodes.Ldc_I4, i);
        }
    }
}