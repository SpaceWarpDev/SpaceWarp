using System;
using System.Collections.Generic;
using HarmonyLib;
using KSP.Game;
using KSP.Game.Load;
using KSP.Iteration.UI.Binding;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.Sim.impl;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UniLinq;
using Enumerable = System.Linq.Enumerable;

namespace SpaceWarp.Patching;



internal static class LoadVehiclesPatch
{
    //
    // private static void ThrowIfNull(PartCore core, SerializedPart part)
    // {
    //     if (core == null)
    //     {
    //         throw new MissingPartException(part.partName);
    //     }
    // }
    //
    // [HarmonyPatch(typeof(SpaceSimulation), nameof(SpaceSimulation.CreatePart))]
    // [HarmonyILManipulator]
    // internal static void CreatePartFix(ILContext ilContext, ILLabel endLabel)
    // {
    //     ILCursor ilCursor = new(ilContext);
    //     var getMethod = AccessTools.GetDeclaredMethods(typeof(PartProvider))
    //         .First(method => method.Name == nameof(PartProvider.Get));
    //     ilCursor.GotoNext(MoveType.After, instruction => instruction.MatchCallOrCallvirt(getMethod));
    //     ilCursor.GotoNext(MoveType.After, instruction => instruction.OpCode == OpCodes.Stloc_2);
    //     ilCursor.Emit(OpCodes.Ldloc_2);
    //     ilCursor.Emit(OpCodes.Ldarg_1);
    //     ilCursor.EmitDelegate(ThrowIfNull);
    // }
    //

    
    private static List<string> VerifyParts(SerializedAssembly vessel) => Enumerable.ToList((from part in vessel.parts where GameManager.Instance.Game.Parts.Get(part.partName) == null select part.partName));

    [HarmonyPatch(typeof(CreateVesselsFlowAction),nameof(CreateVesselsFlowAction.DoAction))]
    [HarmonyPrefix]
    internal static bool LoadVesselFix(CreateVesselsFlowAction __instance, Action resolve, Action<string> reject)
    {
        __instance._game.UI.SetLoadingBarText(__instance.Description);
        var startingState = SaveLoadGameUtil.GetMyStartingGameStateFromSavedGame(__instance._data.SavedGame);
        if (startingState == GameState.FlightView)
        {
            __instance._game.GlobalGameState.SetState(startingState);
        }

        if (__instance._data.SavedGame.Vessels != null)
        {
            var maxDebris = PersistentProfileManager.MaxDebrisCount;
            var vessels = __instance._data.SavedGame.Vessels;
            foreach (var vessel in vessels)
            {
                if (vessel.IsDebris)
                {
                    if (maxDebris > 0)
                    {
                        maxDebris--;
                    }
                    else
                    {
                        continue;
                    }
                }

                var missing = VerifyParts(vessel);
                if (missing.Count > 0)
                {
                    Modules.Game.Instance.ModuleLogger.LogError(
                        $"Skipping vehicle: {vessel.AssemblyDefinition.assemblyName} due to the following missing parts");
                    foreach (var missingPart in missing)
                    {
                        Modules.Game.Instance.ModuleLogger.LogError($"- {missingPart}");
                    }
                    continue;
                }
                
                var playerGuid = vessel.OwnerPlayerGuidString;
                var playerId = vessel.OwnerPlayerId;
                var authority = vessel.AuthorityPlayerId;
                var loadedObject =
                    __instance._game.SpaceSimulation.CreateVesselSimObject(vessel, playerGuid, playerId, authority);
                var vesselComponent = loadedObject.Vessel;
                vesselComponent?.LoadedFromSaveFile();
                loadedObject.ObservedCameraGimbal = vessel.CameraGimbalSate;
                GameManager.Instance.Game.OAB.GetNextPartSeedCounterValueAndAdvance();
                __instance._data.legacyData.Add(new Tuple<string, List<SerializedPart>>(loadedObject.GlobalIdGuidString, vessel.parts));
                __instance._game.KeepAliveNetworkPump();

            } 
        }

        var observedGuid = SaveLoadGameUtil.GetMyObservedSimObjectGuidFromSavedGame(__instance._data.SavedGame);
        if (observedGuid == IGGuid.Empty)
        {
            __instance._game.GlobalGameState.SetState(GameState.KerbalSpaceCenter);
        }
        else
        {
            try
            {
                __instance._game.ViewController.FlightObserver.ObserveSimObject(observedGuid);
                var origin = GameManager.Instance.Game.UniverseView.PhysicsSpace.FloatingOrigin;
                if (origin != null)
                {
                    origin.IsPendingForceSnap = true;
                }
            }
            catch
            {
                __instance._game.GlobalGameState.SetState(GameState.KerbalSpaceCenter);
            }
        }
        resolve();
        return true;
    }
}