using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SpaceWarp.API.Mods.JSON;

[JsonConverter(typeof(StringEnumConverter))]
public enum VersionCheckType
{
    [EnumMember(Value = "swinfo")]
    SwInfo,
    [EnumMember(Value = "csproj")]
    Csproj
}