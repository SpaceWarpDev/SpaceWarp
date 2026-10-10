using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace SpaceWarp2.API.Mods.JSON;

/// <summary>
/// One entry of the <c>features</c> section of a mod info file.
/// </summary>
/// <remarks>
/// Ids without a <c>:</c> refer to features of the declaring mod. Qualified ids, <c>ModId:FeatureId</c>, refer to
/// features of other mods.
/// </remarks>
[JsonObject(MemberSerialization.OptIn)]
[PublicAPI]
public class FeatureInfo
{
    /// <summary>
    /// The features this feature turns on with it.
    /// </summary>
    [JsonProperty("requires", Required = Required.DisallowNull)]
    public List<string> Requires { get; internal set; } = new();

    /// <summary>
    /// The features that cannot be on in the same save as this one.
    /// </summary>
    [JsonProperty("conflicts", Required = Required.DisallowNull)]
    public List<string> Conflicts { get; internal set; } = new();
}
