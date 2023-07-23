using System.Collections.Generic;
using BepInEx;
using SpaceWarp.Patching;
using UnityEngine;

namespace SpaceWarp.API.Mods;

public class SpaceWarpErrorDescription
{
    public SpaceWarpPluginDescriptor Plugin;
    public bool MissingSwinfo = false;
    public bool BadDirectory = false;
    public bool BadID = false;
    public bool MismatchedVersion = false;

    public List<string> DisabledDependencies = new();
    public List<string> ErroredDependencies = new();
    public List<string> MissingDependencies = new();
    public List<string> UnsupportedDependencies = new();
    public List<string> UnspecifiedDependencies = new();

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
                    Object.Destroy(unityPlugin);
                    break;
            }
        }
        plugin.Plugin = null;
    }

}