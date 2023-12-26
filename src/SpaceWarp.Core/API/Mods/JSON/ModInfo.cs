using JetBrains.Annotations;
using Newtonsoft.Json;

namespace SpaceWarp.API.Mods.JSON;

/// <summary>
/// Representation of the mod info JSON file.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
[PublicAPI]
public sealed class ModInfo
{
    [JsonProperty("spec", Required = Required.DisallowNull)] public SpecVersion Spec { get; internal set; } = new();

    [JsonProperty("mod_id", Required = Required.DisallowNull)] private string _modID = "";

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


    [JsonProperty("name")]
    private string _name;
    public string Name
    {
        get => _name ?? _modID;
        internal set => _name = value;
    }

    [JsonProperty("author", Required = Required.DisallowNull)]
    public string Author { get; internal set; } = "";

    [JsonProperty("description", Required = Required.DisallowNull)] public string Description { get; internal set; } = "";

    [JsonProperty("source", Required = Required.DisallowNull)] public string Source { get; internal set; } = "";

    [JsonProperty("version", Required = Required.Always)] public string Version { get; internal set; }

    [JsonProperty("dependencies", Required = Required.DisallowNull)] public List<DependencyInfo> Dependencies { get; internal set; } = new();

    [JsonProperty("ksp2_version", Required = Required.DisallowNull)]
    public SupportedVersionsInfo SupportedKsp2Versions { get; internal set; } = new()
    {
        Min = "*",
        Max = "*"
    };

    [JsonProperty("version_check")]
    [CanBeNull]
    public string VersionCheck { get; internal set; }

    [JsonProperty("version_check_type")]
    [Obsolete("Only swinfo.json version checking will be allowed in 2.0.0.")]
    public VersionCheckType VersionCheckType { get; internal set; } = VersionCheckType.SwInfo;

    [JsonProperty("conflicts", Required = Required.DisallowNull)]
    public List<DependencyInfo> Conflicts { get; internal set; } = new();
}