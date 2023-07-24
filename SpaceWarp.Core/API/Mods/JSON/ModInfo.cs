using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace SpaceWarp.API.Mods.JSON;

/// <summary>
///     Representation of the mod info JSON file.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public sealed class ModInfo
{
    [JsonProperty("spec")] public SpecVersion Spec { get; internal set; } = new();

    [JsonProperty("mod_id")] private string _modID;

    public string ModID
    {
        get
        {
            if (Spec == SpecVersion.V1_2)
            {
                throw new DeprecatedSwinfoPropertyException(nameof(ModID), SpecVersion.V1_2);
            }
            return _modID;
        }
        internal set => _modID = value;
    }

    [JsonProperty("name")] public string Name { get; internal set; }

    [JsonProperty("author")] public string Author { get; internal set; }

    [JsonProperty("description")] public string Description { get; internal set; }

    [JsonProperty("source")] public string Source { get; internal set; }

    [JsonProperty("version")] public string Version { get; internal set; }

    [JsonProperty("dependencies")] public List<DependencyInfo> Dependencies { get; internal set; }

    [JsonProperty("ksp2_version")] public SupportedVersionsInfo SupportedKsp2Versions { get; internal set; }

    [JsonProperty("version_check")]
    [CanBeNull]
    public string VersionCheck { get; internal set; }

    [JsonProperty("version_check_type")]
    public VersionCheckType VersionCheckType { get; internal set; } = VersionCheckType.SwInfo;
}