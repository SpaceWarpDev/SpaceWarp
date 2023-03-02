using Newtonsoft.Json;
using System.Collections.Generic;

namespace SpaceWarp.API.Mods.JSON;

/// <summary>
/// Representation of the mod info JSON file.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public sealed class ModInfo
{
    [JsonProperty("mod_id")] 
    public string ModID { get; internal set; }

    [JsonProperty("name")]
    public string Name { get; internal set; }

    [JsonProperty("author")] 
    public string Author { get; internal set; }

    [JsonProperty("description")] 
    public string Description { get; internal set; }

    [JsonProperty("source")] 
    public string Source { get; internal set; }

    [JsonProperty("version")] 
    public string Version { get; internal set; }

    [JsonProperty("dependencies")]
    public List<DependencyInfo> Dependencies { get; internal set; }

    [JsonProperty("ksp2_version")]
    public SupportedVersionsInfo SupportedKsp2Versions { get; internal set; }
}
