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
    /// <summary>
    /// The spec version of the mod info file.
    /// </summary>
    [JsonProperty("spec", Required = Required.DisallowNull)]
    public SpecVersion Spec { get; internal set; } = new();

    [JsonProperty("mod_id", Required = Required.DisallowNull)]
    private string _modID = "";

    /// <summary>
    /// The mod ID of the mod.
    /// </summary>
    /// <exception cref="DeprecatedSwinfoPropertyException">Thrown when the spec version is 1.2.</exception>
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

    /// <summary>
    /// The name of the mod.
    /// </summary>
    public string Name
    {
        get => _name ?? _modID;
        internal set => _name = value;
    }

    /// <summary>
    /// The author of the mod.
    /// </summary>
    [JsonProperty("author", Required = Required.DisallowNull)]
    public string Author { get; internal set; } = "";

    /// <summary>
    /// The description of the mod.
    /// </summary>
    [JsonProperty("description", Required = Required.DisallowNull)]
    public string Description { get; internal set; } = "";

    /// <summary>
    /// The URL of the source code of the mod.
    /// </summary>
    [JsonProperty("source", Required = Required.DisallowNull)]
    public string Source { get; internal set; } = "";

    /// <summary>
    /// The version of the mod.
    /// </summary>
    [JsonProperty("version", Required = Required.Always)]
    public string Version { get; internal set; }

    /// <summary>
    /// The dependencies of the mod.
    /// </summary>
    [JsonProperty("dependencies", Required = Required.DisallowNull)]
    public List<DependencyInfo> Dependencies { get; internal set; } = new();

    /// <summary>
    /// The KSP2 versions supported by the mod.
    /// </summary>
    [JsonProperty("ksp2_version", Required = Required.DisallowNull)]
    public SupportedVersionsInfo SupportedKsp2Versions { get; internal set; } = new()
    {
        Min = "*",
        Max = "*"
    };

    /// <summary>
    /// The URL for the version checking of the mod.
    /// </summary>
    [JsonProperty("version_check")]
    [CanBeNull]
    public string VersionCheck { get; internal set; }

    /// <summary>
    /// The type of version checking of the mod.
    /// </summary>
    [JsonProperty("version_check_type")]
    [Obsolete("Only swinfo.json version checking will be allowed in 2.0.0.")]
    public VersionCheckType VersionCheckType { get; internal set; } = VersionCheckType.SwInfo;

    /// <summary>
    /// The conflicts of the mod.
    /// </summary>
    [JsonProperty("conflicts", Required = Required.DisallowNull)]
    public List<DependencyInfo> Conflicts { get; internal set; } = new();

    /// <summary>
    /// The filenames of patcher assemblies of the mod.
    /// </summary>
    [JsonProperty("patchers", Required = Required.DisallowNull)]
    public List<string> Patchers { get; internal set; } = new();
}