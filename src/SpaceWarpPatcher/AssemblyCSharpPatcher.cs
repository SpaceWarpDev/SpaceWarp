using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using MonoMod.Cil;
using MonoMod.Utils;

namespace SpaceWarpPatcher;

/// <summary>
/// Patcher for the game's main DLL
/// </summary>
[UsedImplicitly]
public class AssemblyCSharpPatcher
{
    /// <summary>
    /// The target DLLs to patch
    /// </summary>
    [UsedImplicitly]
    public static IEnumerable<string> TargetDLLs => new[] { "Assembly-CSharp.dll"};

    /// <summary>
    /// Patches the target DLL
    /// </summary>
    /// <param name="assemblyDefinition">The assembly definition to patch</param>
    [UsedImplicitly]
    public static void Patch(ref AssemblyDefinition assemblyDefinition)
    {
        // AssemblyDefinition coreAssembly = null;
        // var dir = new DirectoryInfo(Paths.PluginPath);
        // foreach (var file in dir.EnumerateFiles("SpaceWarp.Core.dll", SearchOption.AllDirectories))
        // {
        //     coreAssembly = AssemblyDefinition.ReadAssembly(file.FullName);
        // }
        //
        // if (coreAssembly == null)
        // {
        //     throw new Exception("Could not find SpaceWarp Core");
        // }

        var firstTargetType = assemblyDefinition.MainModule.Types.First(t => t.Name == "PartOwnerComponent");
        var boolType = firstTargetType.Fields
            .Select(x => x.FieldType).First(x => x.MetadataType == MetadataType.Boolean)!;
        firstTargetType.Fields.Add(new FieldDefinition("HasRegisteredPartComponentsForFixedUpdate",FieldAttributes.Public,boolType));
        var field = firstTargetType.Fields.First(x => x.Name == "HasRegisteredPartComponentsForFixedUpdate");
        // Now later we harmony patch the initializer for partownercomponent

        var targetMethod = firstTargetType.Methods.First(method => method.Name == "OnFixedUpdate");
        var methodCallA = assemblyDefinition.MainModule.Types.First(t => t.Name == "ResourceFlowRequestManager").Methods
            .First(m => m.Name == "UpdateFlowRequests");
        var methodCallB = firstTargetType.Properties
            .First(definition => definition.Name == "ResourceFlowRequestManager").GetMethod;
        // var context = new ILContext(targetMethod);
        // var cursor = new ILCursor(context);
        // var elseEndLabel  = cursor.DefineLabel();
        // var elseBeginLabel = cursor.DefineLabel();
        // cursor.GotoNext(MoveType.Before)
        // cursor.GotoNext(MoveType.After, instruction => instruction.MatchCallOrCallvirt(methodCallA));
        var insts = targetMethod.Body.Instructions;
        Collection<Instruction> newInstructions = new Collection<Instruction>();
        var nextIsTarget = false;
        Instruction done = null;
        Instruction @else = null;
        foreach (var currentInstruction in insts)
        {
            if (nextIsTarget)
            {
                done = currentInstruction;
                nextIsTarget = false;
            }
            newInstructions.Add(currentInstruction);
            if (!currentInstruction.MatchCallOrCallvirt(methodCallA)) continue;
            nextIsTarget = true;
            newInstructions.Add(Instruction.Create(OpCodes.Nop));
            newInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            @else = newInstructions.Last();
            newInstructions.Add(Instruction.Create(OpCodes.Ldfld,field));
            newInstructions.Add(Instruction.Create(OpCodes.Ldarg_3));
            newInstructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            newInstructions.Add(Instruction.Create(OpCodes.Call, methodCallB));
            newInstructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            newInstructions.Add(Instruction.Create(OpCodes.Ldarg_2));
            newInstructions.Add(Instruction.Create(OpCodes.Callvirt, methodCallA));
        }

        var isFirstJump = true;
        for (var i = 0; i < newInstructions.Count; i++)
        {
            var currentInstruction = newInstructions[i];
            if (currentInstruction.OpCode == OpCodes.Brfalse_S)
            {
                if (isFirstJump)
                {
                    isFirstJump = false;
                }
                else
                {
                    newInstructions[i] = Instruction.Create(OpCodes.Brfalse_S, @else);
                }
            }

            if (currentInstruction.OpCode == OpCodes.Bne_Un_S)
            {
                newInstructions[i] = Instruction.Create(OpCodes.Bne_Un_S, @else);
            }

            if (currentInstruction.OpCode == OpCodes.Nop)
            {
                newInstructions[i] = Instruction.Create(OpCodes.Br_S, done);
            }

            if (currentInstruction.OpCode == OpCodes.Ldarg_3)
            {
                newInstructions[i] = Instruction.Create(OpCodes.Brfalse_S, done);
            }
        }


        insts.Clear();
        insts.AddRange(newInstructions);
    }
}