using JetBrains.Annotations;
using Newtonsoft.Json;
using SpaceWarp.API.Versions;

namespace SpaceWarp.API.Mods.JSON;

/// <summary>
/// Representation of the supported version info of a mod from a JSON file.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
[PublicAPI]
public sealed class SupportedVersionsInfo
{
    /// <summary>
    /// The default minimum version.
    /// </summary>
    public const string DefaultMin = "0.0.0";

    /// <summary>
    /// The default maximum version.
    /// </summary>
    public const string DefaultMax = "*";

    /// <summary>
    /// The minimum supported version of KSP2.
    /// </summary>
    [JsonProperty("min")] public string Min { get; internal set; } = DefaultMin;

    /// <summary>
    /// The maximum supported version of KSP2.
    /// </summary>
    [JsonProperty("max")] public string Max { get; internal set; } = DefaultMax;

    /// <summary>
    /// Checks if the given version is supported by this mod.
    /// </summary>
    /// <param name="toCheck">The version to check.</param>
    /// <returns></returns>
    public bool IsSupported(string toCheck)
    {
        return VersionUtility.IsSupported(toCheck, Min, Max);
    }

    /// <summary>
    /// Returns a string representation of the version range.
    /// </summary>
    /// <returns>The string representation of the version range.</returns>
    public override string ToString()
    {
        return $"{Min} - {Max}";
    }
}