using Newtonsoft.Json;

namespace SpaceWarp.API.Configuration
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DependencyInfo
    {
        [JsonProperty] public string id { get; set; }
        [JsonProperty] public VersionInfo version { get; set; }
    }
}