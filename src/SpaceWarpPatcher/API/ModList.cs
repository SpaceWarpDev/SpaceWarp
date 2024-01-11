using BepInEx;

namespace SpaceWarpPatcher.API;

/// <summary>
/// Contains methods related to the mod list.
/// </summary>
internal static class ModList
{
    internal static string DisabledPluginsFilepath => Path.Combine(Paths.BepInExRootPath, "disabled_plugins.cfg");

    /// <summary>
    /// The list of all disabled plugin GUIDs.
    /// </summary>
    internal static string[] DisabledPluginGuids { get; private set; }

    /// <summary>
    /// The list of all disabled plugins.
    /// </summary>
    internal static List<PluginInfo> DisabledPlugins { get; } = [];

    /// <summary>
    /// Whether the mod list changed since the last run.
    /// </summary>
    public static bool ChangedSinceLastRun { get; internal set; }

    internal static void Initialize()
    {
        if (!File.Exists(DisabledPluginsFilepath))
        {
            File.Create(DisabledPluginsFilepath).Dispose();
            Patcher.LogSource.LogWarning(
                $"Disabled plugins file did not exist, created empty file at: {DisabledPluginsFilepath}"
            );
        }

        DisabledPluginGuids = File.ReadAllLines(DisabledPluginsFilepath);
    }
}