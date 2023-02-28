using Newtonsoft.Json;

namespace SpaceWarp.API.Mods.JSON;

/// <summary>
/// Represents the json property info. Properties have to use the same name as in the JSON file, that's why they break convention.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class DependencyInfo
{
    [JsonProperty("id")]
    public string id { get; set; }

    [JsonProperty("version")] 
    public SupportedVersionsInfo version { get; set; }
}