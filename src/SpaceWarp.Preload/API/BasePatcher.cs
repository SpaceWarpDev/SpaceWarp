using BepInEx.Logging;
using JetBrains.Annotations;
using Mono.Cecil;

namespace SpaceWarp.Preload.API;

/// <summary>
/// Base class of a SpaceWarp wrapper for BepInEx preload patchers. Runs conditionally based on whether the mod is
/// enabled.
/// </summary>
[PublicAPI]
public abstract class BasePatcher : IPatcher
{
    /// <summary>
    /// Logger for the patcher.
    /// </summary>
    protected internal ManualLogSource Logger;

    /// <inheritdoc/>
    public abstract IEnumerable<string> DLLsToPatch { get; }

    /// <inheritdoc/>
    public abstract void ApplyPatch(ref AssemblyDefinition assembly);
}