using JetBrains.Annotations;

namespace SpaceWarp.API.Mods;

[PublicAPI]
public static class GlobalModDefines
{
    public static readonly string AssetBundlesFolder = Path.Combine("assets", "bundles");
    public static readonly string ImageAssetsFolder = Path.Combine("assets", "images");
}