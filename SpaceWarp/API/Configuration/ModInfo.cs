using System.Collections.Generic;
using Newtonsoft.Json;
namespace SpaceWarp.API.Configuration
{
    
    [JsonObject(MemberSerialization.OptIn)]
    public class ModInfo
    {
        [JsonProperty] public string mod_id { get; set; }
        [JsonProperty] public string name { get; set; }
        [JsonProperty] public string author { get; set; }
        [JsonProperty] public string description { get; set; }
        [JsonProperty] public string source { get; set; }
        [JsonProperty] public string version { get; set; }
        [JsonProperty] public List<DependencyInfo> dependencies { get; set; }
        [JsonProperty] public VersionInfo ksp2_version { get; set; }
    }
}