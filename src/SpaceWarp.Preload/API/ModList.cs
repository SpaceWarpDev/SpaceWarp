using BepInEx;

namespace SpaceWarp.Preload.API;

/// <summary>
/// Contains methods related to the mod list.
/// </summary>
internal static class ModList
{
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
        if (!File.Exists(CommonPaths.DisabledPluginsFilepath))
        {
            File.Create(CommonPaths.DisabledPluginsFilepath).Dispose();
            Entrypoint.LogSource.LogWarning(
                $"Disabled plugins file did not exist, created empty file at: {CommonPaths.DisabledPluginsFilepath}"
            );
        }

        DisabledPluginGuids = File.ReadAllLines(CommonPaths.DisabledPluginsFilepath);
    }
}