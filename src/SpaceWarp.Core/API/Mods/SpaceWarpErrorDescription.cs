using System.Collections.Generic;
using BepInEx;
using JetBrains.Annotations;
using SpaceWarp.Patching;
using UnityEngine;

namespace SpaceWarp.API.Mods;

[PublicAPI]
public class SpaceWarpErrorDescription
{
    public SpaceWarpPluginDescriptor Plugin;
    public bool MissingSwinfo;
    public bool BadDirectory;
    public bool BadID;
    public bool MismatchedVersion;

    public List<string> DisabledDependencies = new();
    public List<string> ErroredDependencies = new();
    public List<string> MissingDependencies = new();
    public List<string> UnsupportedDependencies = new();
    public List<string> UnspecifiedDependencies = new();
    public List<string> Incompatibilities = new();

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