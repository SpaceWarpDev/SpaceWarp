using Newtonsoft.Json;
namespace SpaceWarp.API
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GlobalConfiguration
    {
        [JsonProperty] public int LogLevel { get; set; }
    }
}