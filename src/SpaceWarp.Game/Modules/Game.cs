using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using KSP.Game;
using KSP.Messages;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Game.Messages;
using SpaceWarp.API.Game.Waypoints;
using UnityEngine;
using ILogger = SpaceWarp.API.Logging.ILogger;
using Object = UnityEngine.Object;

namespace SpaceWarp.Modules;

/// <summary>
/// The module for game-related APIs.
/// </summary>
[UsedImplicitly]
public class Game : SpaceWarpModule
{

    internal static ILogger Logger;
    /// <inheritdoc />
    public override string Name => "SpaceWarp.Game";

    /// <inheritdoc />
    public override void PreInitializeModule()
    {
        Logger = ModuleLogger;
        Harmony.CreateAndPatchAll(typeof(Patches.MapPatches));
    }

    /// <inheritdoc />
    public override void InitializeModule()
    {
        var game = GameManager.Instance.Game;
        game.Messages.Subscribe(typeof(GameStateEnteredMessage), StateChanges.OnGameStateEntered, false, true);
        game.Messages.Subscribe(typeof(GameStateLeftMessage), StateChanges.OnGameStateLeft, false, true);
        game.Messages.Subscribe(typeof(GameStateChangedMessage), StateChanges.OnGameStateChanged, false, true);
        game.Messages.Subscribe(typeof(TrackingStationLoadedMessage), StateLoadings.TrackingStationLoadedHandler, false, true);
        game.Messages.Subscribe(typeof(TrackingStationUnloadedMessage), StateLoadings.TrackingStationUnloadedHandler, false, true);
        game.Messages.Subscribe(typeof(TrainingCenterLoadedMessage), StateLoadings.TrainingCenterLoadedHandler, false, true);
    }
}