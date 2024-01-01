using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SpaceWarp.Backend.Modding;

/// <summary>
/// Represents the KSP2 mod info file used by the internal loader.
/// </summary>
public class Ksp2ModInfo
{
    /// <summary>
    /// The API version of the mod.
    /// </summary>
    [JsonConverter(typeof(VersionConverter))]
    [JsonProperty]
    public Version APIVersion { get; private set; }

    /// <summary>
    /// The version of the mod.
    /// </summary>
    [JsonConverter(typeof(VersionConverter))]
    [JsonProperty]
    public Version ModVersion { get; private set; }

    /// <summary>
    /// The name of the mod.
    /// </summary>
    [JsonProperty]
    public string ModName { get; private set; }

    /// <summary>
    /// The author of the mod.
    /// </summary>
    [JsonProperty]
    public string ModAuthor { get; private set; }

    /// <summary>
    /// The description of the mod.
    /// </summary>
    [JsonProperty]
    public string ModDescription { get; private set; }

    /// <summary>
    /// The addressables catalog file of the mod.
    /// </summary>
    [JsonProperty]
    public string Catalog { get; private set; }
}