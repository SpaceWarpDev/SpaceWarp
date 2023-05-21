using Newtonsoft.Json;
using SpaceWarp.API.Versions;

namespace SpaceWarp.API.Mods.JSON;

/// <summary>
///     Representation of the supported version info of a mod from a JSON file.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public sealed class SupportedVersionsInfo
{
    internal const string DefaultMin = "0.0.0";
    internal const string DefaultMax = "*";

    [JsonProperty("min")] public string Min { get; internal set; } = DefaultMin;

    [JsonProperty("max")] public string Max { get; internal set; } = DefaultMax;

    public bool IsSupported(string toCheck)
    {
        return VersionUtility.IsSupported(toCheck, Min, Max);
    }

    public override string ToString()
    {
        return $"{Min} - {Max}";
    }
}