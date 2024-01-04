using JetBrains.Annotations;

namespace SpaceWarp.API.Mods;

/// <summary>
/// Global definitions for all SpaceWarp mods.
/// </summary>
[PublicAPI]
public static class GlobalModDefines
{
    /// <summary>
    /// Relative path to the folder containing the mod's asset bundles.
    /// </summary>
    public static readonly string AssetBundlesFolder = Path.Combine("assets", "bundles");
    /// <summary>
    /// Relative path to the folder containing the mod's images.
    /// </summary>
    public static readonly string ImageAssetsFolder = Path.Combine("assets", "images");
}