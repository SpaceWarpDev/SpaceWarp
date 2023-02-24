using Newtonsoft.Json;

namespace SpaceWarp.API.Configuration
{
    [JsonObject(MemberSerialization.OptIn)]
    public class VersionInfo
    {
        [JsonProperty] public string min { get; set; }
        [JsonProperty] public string max { get; set; }
    }
}