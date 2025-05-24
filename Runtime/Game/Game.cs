using JetBrains.Annotations;
using SpaceWarp.Modules;
using ILogger = ReduxLib.Logging.ILogger;

namespace SpaceWarp.Game;

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