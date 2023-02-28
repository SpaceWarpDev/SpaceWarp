using Newtonsoft.Json;

namespace SpaceWarp.API.Mods.JSON;

/// <summary>
/// Representation of the supported version info of a mod from a JSON file.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class SupportedVersionsInfo
{
    [JsonProperty("min")]
    public string min { get; set; } = "0.0.0";

    [JsonProperty("max")]
    public string max { get; set; } = "*";
}