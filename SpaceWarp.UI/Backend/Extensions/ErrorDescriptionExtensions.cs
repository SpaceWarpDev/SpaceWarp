using SpaceWarp.API.Mods;
using SpaceWarp.UI.ModList;

namespace SpaceWarp.Backend.Extensions;

internal static class ErrorDescriptionExtensions
{
    internal static void Apply(this SpaceWarpErrorDescription instance, ModListItemController controller)
    {
        controller.SetInfo(instance.Plugin);
        controller.SetIsErrored();
        if (instance.MissingSwinfo) controller.SetMissingSWInfo();
        if (instance.BadDirectory) controller.SetBadDirectory();
        if (instance.BadID) controller.SetBadID();
        if (instance.MismatchedVersion) controller.SetMismatchedVersion();
        foreach (var disabledDependency in instance.DisabledDependencies)
        {
            controller.SetIsDependencyDisabled(disabledDependency);
        }

        foreach (var erroredDependency in instance.ErroredDependencies)
        {
            controller.SetIsDependencyErrored(erroredDependency);
        }

        foreach (var missingDependency in instance.MissingDependencies)
        {
            controller.SetIsDependencyMissing(missingDependency);
        }

        foreach (var unsupportedDependency in instance.UnsupportedDependencies)
        {
            controller.SetIsDependencyUnsupported(unsupportedDependency);
        }

        foreach (var unspecifiedDependency in instance.UnspecifiedDependencies)
        {
            controller.SetIsDependencyUnspecified(unspecifiedDependency);
        }
    }
}