using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace SpaceWarp.UI.Console;	

public sealed class SpaceWarpConsoleLogListener : ILogListener	
{	
    internal static readonly List<string> DebugMessages = new();	
    private readonly SpaceWarpPlugin _spaceWarpPluginInstance;

    public static event Action<string> OnNewMessage;
    public static event Action<LogInfo> OnNewLog;

    public SpaceWarpConsoleLogListener(SpaceWarpPlugin spaceWarpPluginInstance)	
    {	
        _spaceWarpPluginInstance = spaceWarpPluginInstance;	
    }	

    public void LogEvent(object sender, LogEventArgs eventArgs)	
    {	
        DebugMessages.Add(BuildMessage(TimestampMessage(), eventArgs.Level, eventArgs.Data, eventArgs.Source));

        // Notify all listeners that a new message has been added
        OnNewMessage?.Invoke(DebugMessages[^1]);
        OnNewLog?.Invoke(new LogInfo { dateTime = DateTime.Now, args = eventArgs});

        LogMessageJanitor();
    }	

    public struct LogInfo
    {
        public DateTime dateTime;
        public LogLevel Level => args.Level;
        public ILogSource Source => args.Source;
        public object Data => args.Data;
        public LogEventArgs args;
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