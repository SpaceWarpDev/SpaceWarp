using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SpaceWarp.API.Mods.JSON;

[JsonConverter(typeof(StringEnumConverter))]
[Obsolete("Only swinfo.json version checking will be allowed in 2.0.0.")]
[PublicAPI]
public enum VersionCheckType
{
    [EnumMember(Value = "swinfo")]
    SwInfo,
    [EnumMember(Value = "csproj")]
    Csproj
}