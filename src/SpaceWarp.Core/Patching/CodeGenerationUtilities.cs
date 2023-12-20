using HarmonyLib;
using System.Reflection.Emit;

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
        return i switch
        {
            0 => new CodeInstruction(OpCodes.Ldc_I4_0),
            1 => new CodeInstruction(OpCodes.Ldc_I4_1),
            2 => new CodeInstruction(OpCodes.Ldc_I4_2),
            3 => new CodeInstruction(OpCodes.Ldc_I4_3),
            4 => new CodeInstruction(OpCodes.Ldc_I4_4),
            5 => new CodeInstruction(OpCodes.Ldc_I4_5),
            6 => new CodeInstruction(OpCodes.Ldc_I4_6),
            7 => new CodeInstruction(OpCodes.Ldc_I4_7),
            8 => new CodeInstruction(OpCodes.Ldc_I4_8),
            _ => new CodeInstruction(OpCodes.Ldc_I4, i)
        };
    }
}