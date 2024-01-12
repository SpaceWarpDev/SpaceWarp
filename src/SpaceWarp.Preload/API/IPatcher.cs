using JetBrains.Annotations;
using Mono.Cecil;

namespace SpaceWarp.Preload.API;

/// <summary>
/// SpaceWarp wrapper for BepInEx preload patchers. Runs conditionally based on whether the mod is enabled.
/// </summary>
[PublicAPI]
public interface IPatcher
{
    /// <summary>
    /// The target DLLs to patch.
    /// </summary>
    public IEnumerable<string> DLLsToPatch { get; }

    /// <summary>
    /// Patch the target assembly.
    /// </summary>
    /// <param name="assembly">The target assembly.</param>
    public void ApplyPatch(ref AssemblyDefinition assembly);
}