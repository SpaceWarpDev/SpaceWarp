using BepInEx;
using JetBrains.Annotations;
using SpaceWarp.Patching;

namespace SpaceWarp.API.Mods;

/// <summary>
/// This class is used to describe the errors that occur when loading a plugin
/// </summary>
[PublicAPI]
public class SpaceWarpErrorDescription
{
    /// <summary>
    /// The plugin that errored
    /// </summary>
    public SpaceWarpPluginDescriptor Plugin;

    /// <summary>
    /// Whether the plugin is missing a swinfo file
    /// </summary>
    public bool MissingSwinfo;

    /// <summary>
    /// Whether the plugin is in the wrong directory
    /// </summary>
    public bool BadDirectory;

    /// <summary>
    /// Whether the plugin has a bad ID
    /// </summary>
    public bool BadID;

    /// <summary>
    /// Whether there's a version mismatch between the swinfo and the plugin
    /// </summary>
    public bool MismatchedVersion;

    /// <summary>
    /// List of dependencies that are disabled
    /// </summary>
    public List<string> DisabledDependencies = new();

    /// <summary>
    /// List of dependencies that errored
    /// </summary>
    public List<string> ErroredDependencies = new();

    /// <summary>
    /// List of dependencies that are missing
    /// </summary>
    public List<string> MissingDependencies = new();

    /// <summary>
    /// List of dependencies that are unsupported
    /// </summary>
    public List<string> UnsupportedDependencies = new();

    /// <summary>
    /// List of dependencies that are unspecified
    /// </summary>
    public List<string> UnspecifiedDependencies = new();

    /// <summary>
    /// List of incompatibilities
    /// </summary>
    public List<string> Incompatibilities = new();

    /// <summary>
    /// Creates a new error description for a plugin
    /// </summary>
    /// <param name="plugin">The plugin that errored</param>
    public SpaceWarpErrorDescription(SpaceWarpPluginDescriptor plugin)
    {
        Plugin = plugin;
        // Essentially if we have an errored plugin, we delete the plugin code
        if (plugin.Plugin != null)
        {
            switch (plugin.Plugin)
            {
                // If and only if this is space warp we don't destroy it
                // Then we inform our loading patcher to do space warp specially
                case SpaceWarpPlugin:
                    BootstrapPatch.ForceSpaceWarpLoadDueToError = true;
                    BootstrapPatch.ErroredSWPluginDescriptor = plugin;
                    return;
                case BaseUnityPlugin unityPlugin:
                    UnityObject.Destroy(unityPlugin);
                    break;
            }
        }
        plugin.Plugin = null;
    }
}