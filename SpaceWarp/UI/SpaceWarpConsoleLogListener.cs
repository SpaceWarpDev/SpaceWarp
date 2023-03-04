using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace SpaceWarp.UI;

public sealed class SpaceWarpConsoleLogListener : ILogListener
{
    private readonly SpaceWarpPlugin _spaceWarpPluginInstance;

    internal static readonly List<string> DebugMessages = new();

    public SpaceWarpConsoleLogListener(SpaceWarpPlugin spaceWarpPluginInstance)
    {
        _spaceWarpPluginInstance = spaceWarpPluginInstance;
    }

    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        DebugMessages.Add(BuildMessage(TimestampMessage(), eventArgs.Level, eventArgs.Data, eventArgs.Source));
    }

    private string TimestampMessage()
    {
        return _spaceWarpPluginInstance.configShowTimeStamps.Value
            ? "[" + DateTime.Now.ToString(_spaceWarpPluginInstance.configTimeStampFormat.Value) + "] "
            : "";
    }

    private static string BuildMessage(string timestamp, LogLevel level, object data, ILogSource source)
    {
        return level == LogLevel.None
            ? $"{timestamp}[{source.SourceName}] {data}"
            : $"{timestamp}[{level} : {source.SourceName}] {data}";
    }

    public void Dispose()
    {
        DebugMessages.Clear();
    }
}