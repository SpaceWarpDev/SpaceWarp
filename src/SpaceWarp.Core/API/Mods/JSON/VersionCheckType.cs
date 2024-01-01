using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SpaceWarp.API.Mods.JSON;

/// <summary>
/// The type of version checking to use.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
[Obsolete("Only swinfo.json version checking will be allowed in 2.0.0.")]
[PublicAPI]
public enum VersionCheckType
{
    /// <summary>
    /// Use the swinfo.json file to check for updates.
    /// </summary>
    [EnumMember(Value = "swinfo")]
    SwInfo,
    /// <summary>
    /// Use the .csproj file to check for updates.
    /// </summary>
    [EnumMember(Value = "csproj")]
    Csproj
}