using JetBrains.Annotations;
using Newtonsoft.Json;

namespace SpaceWarp.API.Mods.JSON;

/// <summary>
/// Represents the json property info. Properties have to use the same name as in the JSON file, that's why they break
/// convention.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
[PublicAPI]
public sealed class DependencyInfo
{
    /// <summary>
    /// The ID of the dependency.
    /// </summary>
    [JsonProperty("id")] public string ID { get; internal set; }

    /// <summary>
    /// The version of the dependency.
    /// </summary>
    [JsonProperty("version")] public SupportedVersionsInfo Version { get; internal set; }
}