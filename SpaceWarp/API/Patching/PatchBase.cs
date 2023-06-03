namespace SpaceWarp.API.Patching;

public abstract class PatchBase
{

    private Selector _selector;
    internal Selector CachedSelector => _selector ??= Selector;

    public abstract Selector Selector { get; }
}