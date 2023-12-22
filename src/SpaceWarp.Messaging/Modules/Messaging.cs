using JetBrains.Annotations;

namespace SpaceWarp.Modules;

[PublicAPI]
public class Messaging : SpaceWarpModule
{
    public override string Name => "SpaceWarp.Messaging";
    internal static Messaging Instance;
    public override void LoadModule()
    {
        Instance = this;
    }

    public override void PreInitializeModule()
    {
    }

    public override void InitializeModule()
    {
    }

    public override void PostInitializeModule()
    {
    }
}