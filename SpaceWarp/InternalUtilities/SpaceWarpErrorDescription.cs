using System.Collections.Generic;
using SpaceWarp.API.Mods;
using SpaceWarp.Patching;
using SpaceWarp.UI.ModList;
using UnityEngine;
using static UnityEngine.GameObject;

namespace SpaceWarp.InternalUtilities;

internal class SpaceWarpErrorDescription
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
            // If and only if this is space warp we don't destroy it
            // Then we inform our loading patcher to do space warp specially
            if (plugin.Plugin.Info.Metadata.GUID == SpaceWarpPlugin.ModGuid)
            {
                BootstrapPatch.ForceSpaceWarpLoadDueToError = true;
                BootstrapPatch.ErroredSWPluginDescriptor = plugin;
                return;
            }
            Object.Destroy(plugin.Plugin);
        }
        plugin.Plugin = null;
    }

    public void Apply(ModListItemController controller)
    {
        controller.SetInfo(Plugin);
        controller.SetIsErrored();
        if (MissingSwinfo) controller.SetMissingSWInfo();
        if (BadDirectory) controller.SetBadDirectory();
        if (BadID) controller.SetBadID();
        if (MismatchedVersion) controller.SetMismatchedVersion();
        foreach (var disabledDependency in DisabledDependencies)
        {
            controller.SetIsDependencyDisabled(disabledDependency);
        }

        foreach (var erroredDependency in ErroredDependencies)
        {
            controller.SetIsDependencyErrored(erroredDependency);
        }

        foreach (var missingDependency in MissingDependencies)
        {
            controller.SetIsDependencyMissing(missingDependency);
        }
        
        foreach (var unsupportedDependency in UnsupportedDependencies)
        {
            controller.SetIsDependencyUnsupported(unsupportedDependency);
        }

        foreach (var unspecifiedDependency in UnspecifiedDependencies)
        {
            controller.SetIsDependencyUnspecified(unspecifiedDependency);
        }
    }
}