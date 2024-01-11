using JetBrains.Annotations;

namespace SpaceWarp.Preload.API;

/// <summary>
/// Contains paths to various directories.
/// </summary>
[PublicAPI]
public static class CommonPaths
{
    /// <summary>
    /// Path to the game's root directory.
    /// </summary>
    public static string GameRootPath { get; } = Path.GetDirectoryName(
        Environment.GetEnvironmentVariable("DOORSTOP_PROCESS_PATH")
    );

    /// <summary>
    /// Path to the game's Managed directory.
    /// </summary>
    public static string ManagedPath { get; } = Path.Combine(
        GameRootPath,
        $"{Path.GetFileNameWithoutExtension(Environment.GetEnvironmentVariable("DOORSTOP_PROCESS_PATH"))}_Data",
        "Managed"
    );

    /// <summary>
    /// Path to the BepInEx root directory.
    /// </summary>
    public static string BepInExRootPath { get; } = Path.Combine(GameRootPath, "BepInEx");

    /// <summary>
    /// Path to the BepInEx plugins directory.
    /// </summary>
    public static string BepInExPluginsPath { get; } = Path.Combine(BepInExRootPath, "plugins");

    /// <summary>
    /// Path to the internal mod loader Mods directory.
    /// </summary>
    public static string InternalModLoaderPath { get; } = Path.Combine(GameRootPath, "GameData", "Mods");

    /// <summary>
    /// Path to the file containing the list of disabled plugins.
    /// </summary>
    public static string DisabledPluginsFilepath { get; } = Path.Combine(BepInExRootPath, "disabled_plugins.cfg");
}