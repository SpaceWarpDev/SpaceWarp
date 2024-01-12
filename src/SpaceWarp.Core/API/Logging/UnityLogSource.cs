using JetBrains.Annotations;
using UnityEngine;

namespace SpaceWarp.API.Logging;

/// <summary>
/// A logger that logs to Unity's built-in logging system.
/// </summary>
[PublicAPI]
public class UnityLogSource : BaseLogger
{
    /// <summary>
    /// The name of the logger.
    /// </summary>
    public string Name;

    /// <summary>
    /// Creates a new <see cref="UnityLogSource"/> with the given name.
    /// </summary>
    /// <param name="name">The name of the logger.</param>
    public UnityLogSource(string name)
    {
        Name = name;
    }

    /// <inheritdoc />
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