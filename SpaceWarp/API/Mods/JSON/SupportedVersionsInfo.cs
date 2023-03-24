using Newtonsoft.Json;
using SpaceWarp.API.Versions;

namespace SpaceWarp.API.Mods.JSON;

/// <summary>
///     Representation of the supported version info of a mod from a JSON file.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public sealed class SupportedVersionsInfo
{
    [JsonProperty("min")] public string Min { get; internal set; } = "0.0.0";

    [JsonProperty("max")] public string Max { get; internal set; } = "*";

    public bool IsSupported(string toCheck)
    {
        if (VersionUtility.CompareSemanticVersionStrings(toCheck, Min) < 0)
        {
            return false;
        }

        return VersionUtility.CompareSemanticVersionStrings(toCheck, Max) <= 0;
    }
}