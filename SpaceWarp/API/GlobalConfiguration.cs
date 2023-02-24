using System.ComponentModel;
using Newtonsoft.Json;

namespace SpaceWarp.API
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GlobalConfiguration
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)] [DefaultValue((int)Logging.LogLevel.Info)] public int LogLevel { get; set; }
    }
}