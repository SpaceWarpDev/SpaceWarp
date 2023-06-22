using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace SpaceWarp.UI.Console;

internal sealed class SpaceWarpConsoleLogListener : ILogListener	
{	
    internal static readonly List<string> DebugMessages = new();
    internal static readonly List<LogInfo> LogMessages = new();   
    private readonly SpaceWarpPlugin _spaceWarpPluginInstance;

    public static event Action<string> OnNewMessage;
    public static event Action<LogInfo> OnNewLog;

    public SpaceWarpConsoleLogListener(SpaceWarpPlugin spaceWarpPluginInstance)	
    {	
        _spaceWarpPluginInstance = spaceWarpPluginInstance;	
    }	

    public void LogEvent(object sender, LogEventArgs eventArgs)
    {
        var info = new LogInfo { DateTime = DateTime.Now, Args = eventArgs };
        DebugMessages.Add(BuildMessage(TimestampMessage(), eventArgs.Level, eventArgs.Data, eventArgs.Source));
        LogMessages.Add(info);
        // Notify all listeners that a new message has been added
        OnNewMessage?.Invoke(DebugMessages[^1]);
        OnNewLog?.Invoke(info);

        LogMessageJanitor();
    }	

    public struct LogInfo
    {
        public DateTime DateTime;
        public LogLevel Level => Args.Level;
        public ILogSource Source => Args.Source;
        public object Data => Args.Data;
        public LogEventArgs Args;
    }

    public void Dispose()	
    {	
        DebugMessages.Clear();	
    }	

    private void LogMessageJanitor()	
    {	
        var configDebugMessageLimit = _spaceWarpPluginInstance.ConfigDebugMessageLimit.Value;	
        if (DebugMessages.Count > configDebugMessageLimit)	
            DebugMessages.RemoveRange(0, DebugMessages.Count - configDebugMessageLimit);
        if (LogMessages.Count > configDebugMessageLimit)
            LogMessages.RemoveRange(0, LogMessages.Count - configDebugMessageLimit);
    }	

    private string TimestampMessage()	
    {	
        return _spaceWarpPluginInstance.ConfigShowTimeStamps.Value	
            ? "[" + DateTime.Now.ToString(_spaceWarpPluginInstance.ConfigTimeStampFormat.Value) + "] "	
            : "";	
    }	

    private static string BuildMessage(string timestamp, LogLevel level, object data, ILogSource source)	
    {	
        return level == LogLevel.None	
            ? $"{timestamp}[{source.SourceName}] {data}"	
            : $"{timestamp}[{level} : {source.SourceName}] {data}";	
    }	
}