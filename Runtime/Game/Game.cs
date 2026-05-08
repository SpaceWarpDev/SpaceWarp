using JetBrains.Annotations;
using SpaceWarp2.Modules;
using ILogger = ReduxLib.Logging.ILogger;

namespace SpaceWarp2.Game;

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
    }

    /// <inheritdoc />
    public override void InitializeModule()
    {
    }
}