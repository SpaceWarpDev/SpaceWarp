using Newtonsoft.Json;

namespace SpaceWarp.API.Mods.JSON;

/// <summary>
/// Represents the json property info. Properties have to use the same name as in the JSON file, that's why they break convention.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public sealed class DependencyInfo
{
    [JsonProperty("id")]
    public string ID { get; internal set; }

    [JsonProperty("version")] 
    public SupportedVersionsInfo Version { get; internal set; }
}
