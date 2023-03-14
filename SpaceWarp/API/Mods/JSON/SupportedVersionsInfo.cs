using System;
using Newtonsoft.Json;

namespace SpaceWarp.API.Mods.JSON;

/// <summary>
/// Representation of the supported version info of a mod from a JSON file.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public sealed class SupportedVersionsInfo
{
    [JsonProperty("min")]
    public string Min { get; internal set; } = "0.0.0";

    [JsonProperty("max")]
    public string Max { get; internal set; } = "*";

    public bool IsSupported(string toCheck)
    {
        try
        {
            var minList = Min.Split('.');
            var maxList = Max.Split('.');
            var checkList = toCheck.Split('.');
            var minMin = Math.Min(minList.Length, checkList.Length);
            var minMax = Math.Max(maxList.Length, checkList.Length);
            for (var i = 0; i < minMin; i++)
            {
                if (minList[i] == "*")
                {
                    break;
                }
                if (int.Parse(checkList[i]) < int.Parse(minList[i]))
                {
                    return false;
                }
                if (int.Parse(checkList[i]) > int.Parse(minList[i]))
                {
                    break;
                }
            }

            for (var i = 0; i < minMax; i++)
            {
                if (maxList[i] == "*")
                {
                    break;
                }
                if (int.Parse(checkList[i]) > int.Parse(minList[i]))
                {
                    return false;
                }
                if (int.Parse(checkList[i]) < int.Parse(minList[i]))
                {
                    break;
                }
            }

            return true;
        }
        catch
        {
            return true;
        }
    }
}
