using UnityEngine;

namespace SpaceWarp.API.Logging;

public class UnityLogSource : BaseLogger
{
    public string Name;

    public UnityLogSource(string name)
    {
        Name = name;
    }

    public override void Log(LogLevel level, object x)
    {
        switch (level)
        {
            case LogLevel.Fatal:
            case LogLevel.Error:
                Debug.LogError($"{Name}: {x}");
                break;
            case LogLevel.Warning:
                Debug.LogWarning($"{Name}: {x}");
                break;
            case LogLevel.Debug:
            case LogLevel.None:
            case LogLevel.Message:
            case LogLevel.Info:
            case LogLevel.All:
            default:
                Debug.Log($"{Name}: {x}");
                break;
        }
    }
}