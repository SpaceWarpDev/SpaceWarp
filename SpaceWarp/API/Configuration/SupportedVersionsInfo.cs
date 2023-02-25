using Newtonsoft.Json;

namespace SpaceWarp.API.Configuration
{
    /// <summary>
    /// Representation of the supported version info of a mod from a JSON file.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class SupportedVersionsInfo
    {
        [JsonProperty("min")] 
        public string min { get; set; }

        [JsonProperty("max")] 
        public string max { get; set; }
    }
}