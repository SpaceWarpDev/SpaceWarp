using JetBrains.Annotations;

namespace SpaceWarp.Modules;

/// <summary>
/// The messaging module.
/// </summary>
[PublicAPI]
public class Messaging : SpaceWarpModule
{
    /// <inheritdoc />
    public override string Name => "SpaceWarp.Messaging";

    internal static Messaging Instance;

    /// <inheritdoc />
    public override void LoadModule()
    {
        Instance = this;
    }
}