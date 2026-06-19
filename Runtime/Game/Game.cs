using JetBrains.Annotations;
using SpaceWarp2.Modules;
using UnityEngine;
using ILogger = ReduxLib.Logging.ILogger;

namespace SpaceWarp2.Game;

/// <summary>
/// The module for game-related APIs.
/// </summary>
[UsedImplicitly]
public class Game : SpaceWarpModule
{

    internal static ILogger Logger;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Logger = null;
    }

    /// <inheritdoc />
    public override string Name => "SpaceWarp.Game";

    /// <inheritdoc />
    public override void PreInitializeModule()
    {
        Logger = ModuleLogger;
    }

    /// <inheritdoc />
    public override void InitializeModule()
    {
    }
}