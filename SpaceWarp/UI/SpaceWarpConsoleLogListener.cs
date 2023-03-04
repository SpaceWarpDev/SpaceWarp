using System.Collections.Generic;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using UnityEngine;

namespace SpaceWarp.UI;

public sealed class SpaceWarpConsoleLogListener : ILogListener
{
    internal static readonly List<string> DebugMessages = new();

    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        DebugMessages.Add(BuildMessage(eventArgs.Level, eventArgs.Data, eventArgs.Source));
    }

    private static string BuildMessage(LogLevel level, object data, ILogSource source)
    {
        return level == LogLevel.None
            ? $"[{(object) source.SourceName}] {data}"
            : $"[{(object) level} : {(object) source.SourceName}] {data}";
    }

    public void Dispose()
    {
        DebugMessages.Clear();
    }
}