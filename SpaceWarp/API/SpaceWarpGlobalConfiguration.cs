using System;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace SpaceWarp.API;

[JsonObject(MemberSerialization.OptIn)]
public class SpaceWarpGlobalConfiguration
{
    public static SpaceWarpGlobalConfiguration Instance;

    private static string SPACEWARP_CONFIG_FULL_PATH = Path.Combine(SpaceWarpManager.MODS_FULL_PATH,SPACE_WARP_CONFIG_FILE_NAME);
    private const string SPACE_WARP_CONFIG_FILE_NAME = "space_warp_config.json";

    /// <summary>
    /// Loading Global Configuration and puting it into Instance.
    /// </summary>
    public static void Init()
    {
        if (!File.Exists(SPACEWARP_CONFIG_FULL_PATH))
        {
            Instance = new SpaceWarpGlobalConfiguration();
            Instance.ApplyDefaultValues();
        }
        else
        {
            try
            {
                string json = File.ReadAllText(SPACEWARP_CONFIG_FULL_PATH);
                Instance = JsonConvert.DeserializeObject<SpaceWarpGlobalConfiguration>(json);
            }
            catch (Exception exception)
            {
                //TODO: log this in a nicer way, for now I guess we can just construct a new logger
                Debug.LogError($"Loading space warp config failed\nException: {exception}");

                File.Delete(SPACEWARP_CONFIG_FULL_PATH);
                Init();
                return;
            }
        }

        try
        {
            File.WriteAllLines(SPACEWARP_CONFIG_FULL_PATH, new[] { JsonConvert.SerializeObject(Instance) });
        }
        catch (Exception exception)
        {
            //TODO: log this in a nicer way, for now I guess we can just construct a new logger
            Debug.LogError($"Saving the spacewarp config failed\nException: {exception}");
        }
    }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    [DefaultValue((int)Logging.LogLevel.Info)]
    public int LogLevel { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    [DefaultValue((bool)false)]
    public bool HarmonyLoggin { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    [DefaultValue((int)Logging.LogLevel.Info)]
    public int HarmonyLogLevel { get; set; }

    public void ApplyDefaultValues()
    {
        LogLevel = (int)Logging.LogLevel.Info;
        HarmonyLoggin = false;
        HarmonyLogLevel = (int)Logging.LogLevel.Info;
    }
}